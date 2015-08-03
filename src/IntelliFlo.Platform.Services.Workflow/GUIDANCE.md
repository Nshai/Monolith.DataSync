# Building a MicroService

Some information about building a microservice...............

## Principles
 - A......
 - B......
 - C......

## Project Structure

### Host

Contains the startup and bootstrappers for hosting webapi and messaging capabilities.

### Http

General Http related code such as Hal constants or Etag constants etc

### Modules

Autofac Container modules for loading and registering anything that is required to be depenecy injected

### Domain

This folder is for your domain entities. These are the classes that contain your business logic and if you are using and ORM such as
NHibernate these are the classes that will be mapped to schema to provide persistence.

Keep your business logic in these classes and do not let it leak out into other areas. Domains are not versioned but nedd to be 
backward comaptible when introducing new verions.

### v1

This is where you start writing your service code. Specifically this folder/namespace is for:
 - Versioned (v1) message contracts 
 - Resource interfaces and 
 - Message handler interfaces. 

Message contracts are classes that get serialized on the wire, either over HTTP for REST or other SNS supported protocols. 

If you need to create a new version of your service or an endpoint, create a new sibling v2 folder and so on...

### v1/Resources

Keep your resource implementations in here

### v1/Controllers

Keep your Api controllers in here.

### Messaging

Keep your SNS incomming message handlers in here


## Implementation Guidance

Loren impsum doler sit amet. Loren impsum doler sit amet. Loren impsum doler sit amet. Loren impsum doler sit amet.
Loren impsum doler sit amet. Loren impsum doler sit amet.

