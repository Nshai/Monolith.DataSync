// v0.1.2  
// Gulp 4.0 file. 
// Install gulp: http://hazmi.id/gulp-4-is-nearly-there-you-can-start-using-it/
// Example v4 file: https://gist.github.com/demisx/beef93591edc1521330a

// v2 
//var plugins = require('gulp-load-plugins')();
var gulp = require("gulp");
var shell = require('gulp-shell')
var sprintf = require("sprintf-js").sprintf;
var plugins = require('gulp-load-plugins')();
var fs = require('fs-extra');

plugins.gulputil = require("gulp-util");
plugins.sprintf = require("sprintf-js").sprintf;
plugins.objectAssignDeep = require('object-assign-deep');
plugins.nunit = require('gulp-nunit-runner');
var projectSettings = require("./gulpfile.config.js");


var microservice = {
	tasks : {
		nuget : {
			packageDir : 'packages',
			package : 'Gulp.Tools',
			folder : function (nugetObj) {return sprintf('./%s/%s.%s/', nugetObj.packageDir, nugetObj.package, nugetObj.version);},
			file : function (nugetObj, fileName) { return sprintf('%s%s', nugetObj.folder(nugetObj), fileName); },
			source : "https://artifactory.intelliflo.io/artifactory/api/nuget/nuget-virtual"
		},
		files : {
			configFile : 'gulpfile.config.js',
			task2: 'gulpfile.tasks.js'
		}
	}
}

var get = function(obj, key) {
    return key.split(".").reduce(function(o, x) {
        return (typeof o == "undefined" || o === null) ? o : o[x];
    }, obj);
}

microservice.tasks.nuget.version = get(projectSettings, "tasks.nuget.version");
if (microservice.tasks.nuget.version == undefined) 
{
	var defaultGulpToolsVersion = "0.0.12";
	console.log("Using default gulp tools version " + defaultGulpToolsVersion);
	microservice.tasks.nuget.version = defaultGulpToolsVersion;
} else 
{
	console.log("Using custom gulp tools version " + microservice.tasks.nuget.version);
}




// helper function to dump objects properties and valeus
var dumpObject = function(o){
	for(var propName in o) {
		var propValue = o[propName]
		console.log("propname:'" + propName + "'", 'value:'+propValue);
	}
}

gulp.task('restore-tasks', shell.task([
	sprintf('nuget install %s -OutputDirectory %s -source %s -Version %s', microservice.tasks.nuget.package, microservice.tasks.nuget.packageDir, microservice.tasks.nuget.source, microservice.tasks.nuget.version)
]));


function registerTasksFromDict(tasks){
	console.log('Registering tasks with Gulp');
	for(var propName in tasks) {
		var propValue = tasks[propName];
		console.log("Task: name:'" + propName + "'");
		gulp.task(propName, propValue);
	}
};

var LoadGulpTasksOrLog = function(filePath, callbackExists, callbackNotExists) {
	
	try {
		console.log("loading tasks from: "+ filePath);
		stats = fs.lstatSync(filePath);
		callbackExists()
	}
	catch (e) {
		callbackNotExists();
	}
}

var gulpTasks = microservice.tasks.nuget.file(microservice.tasks.nuget, microservice.tasks.files.task2);
var onGulpTasksMissing = function(){
	console.log(sprintf('Microservice tasks are missing. Install them by running "gulp restore-tasks"'));
}

var loadGulpTasks = function(){
	try
	{
		var taskConfig = microservice.tasks.nuget.file(microservice.tasks.nuget, microservice.tasks.files.configFile);
		var settings = require(taskConfig)(projectSettings, plugins);
		registerTasksFromDict(require(gulpTasks)(gulp, plugins, settings));
	}
	catch (err){
		console.log("Error loading tasks from file. Raise this issue with devs");
		console.log(err);
		dumpObject(err);
	}
}

LoadGulpTasksOrLog(gulpTasks, loadGulpTasks, onGulpTasksMissing);