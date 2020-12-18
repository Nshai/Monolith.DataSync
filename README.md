# Workflow

Provides for hosting and management of user-defined (and system) workflows.

## Installation

Build and run the Microservice.Workflow.exe
A shortcut has also been added to the root folder to enable easy starting of the host

For running on __test__ or __uat__ or __prod__ environments we need to following the next steps:

* Create database with name 'afper'
* Add all needed users (TEST\svc_tst_07_pswrkflow etc.)
* Run sql scripts from '%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\SQL\en' in the next order:
  * SqlWorkflowInstanceStoreSchema.sql
  * SqlWorkflowInstanceStoreLogic.sql
  * SqlWorkflowInstanceStoreSchemaUpgrade.sql. 

## Usage

TODO: Write usage instructions

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Rebase local branches before committing to tidy commit history
5. Push to the branch: `git push origin my-new-feature`
6. Submit a pull request :D

Commit messages should follow best practice i.e. http://chris.beams.io/posts/git-commit/

For example...

Capitalized, short (50 chars or less) summary

More detailed explanatory text, if necessary.  Wrap it to about 72
characters or so.  In some contexts, the first line is treated as the
subject of an email and the rest of the text as the body.  The blank
line separating the summary from the body is critical (unless you omit
the body entirely); tools like rebase can get confused if you run the
two together.

Write your commit message in the imperative: "Fix bug" and not "Fixed bug"
or "Fixes bug."  This convention matches up with commit messages generated
by commands like git merge and git revert.

Further paragraphs come after blank lines.

- Bullet points are okay, too

- Typically a hyphen or asterisk is used for the bullet, followed by a
  single space, with blank lines in between, but conventions vary here

- Use a hanging indent

## History

This project was a refactoring of the initial workflow project from Platform Services.  
The new version is completely decoupled and now hosts the workflows in process rather than having a dependency on IIS.

## Unit tests

```
C:\apps\opencover\OpenCover.Console.exe -returntargetcode -target:"C:\apps\nunit271\nunit-console.exe" -targetargs:"test\Microservice.Workflow.Tests\bin\Release\Microservice.Workflow.Tests.dll --framework=4.0 --nodots --nologo --noshadow /xml:dist\UnitTestResults.xml" -output:dist\OpenCoverResults.xml -filter:"-[*Test*]* -[*]*Host* -[*]*Modules* -[*]*Properties.Settings* +[*Microservice.Workflow*]*" -excludebyattribute:"*ExcludeFromCodeCoverage*" -excludebyfile:"*.Designer.cs;*Program.cs" -hideskipped:All -register:"c:\apps\OpenCover\x64\OpenCover.Profiler.dll"
```