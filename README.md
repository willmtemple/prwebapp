# PeerReviewWeb
(Name Subject To Change)

Developed with :heart: and :beer: by [Will Temple](https://www.wtemple.com) at the University of Colorado Boulder. PeerReviewWeb is inspired by [Captain Teach](github.com/brownplt/admiral-edu).

## About PeerReviewWeb

PeerReviewWeb is a platform that supports the integration of Peer Reviews into coursework. During or after submitting work for an assignment in a course, students will review and comment on each others' work.

It supports multiple courses, configurable instruction staff and student rosters, multiple assignments and multi-stage assignments, with configurable assignment group settings, and configurable review parameters including dynamic review forms.

PeerReviewWeb is not and will never be a learning management system (LMS). It will not replace Moodle, Canvas, D2L, BlackBoard, BannerWeb, or any other LMS for those purposes.  PeerReviewWeb is not and will never be an autograding platform. It will not replace Moodle CodeRunner, INGInious, or whatever secret sauce your university uses.

## Building

A Dockerfile is provided. You can use that to launch an instance immediately. Some environment variables are required:

- `ConnectionStrings:DefaultConnection` must contain a valid connectonstring for a mysql/mariadb server where the user has all permissions on the given database and the database has been migrated.
- `Storage:BasePath` must contain a path where PeerReviewWeb can store files.
- `Authentication:Google:ClientId` must contain the client id for a Google OAuth configuration
- `Authentication:Google:ClientSecret` must contain the secret key for a Google OAuth configuration
- `Authorization:OrganizationDomain` must contain the domain (e.g. "colorado.edu") to which to grant acces to the application (all other domains are prohibited in Production mode, but not in Development mode)
- `Authorization:AdminEmailAddress` must contain the email address (in the given domain) of the application administrator (who will have an extended set of privileges on the server)

### Migration

Clone this repo, and in the base directory, do:

`env "ConnectionStrings:DefaultConnection"=<connection-string> dotnet ef database update`

_Beware passwords with semicolons or quotes in them._

If you have _already started_ a container, you should be able to execute the migration in it. Note that you have to run the migration prior to first accessing the application.

`docker exec -it <container name> dotnet ef database update`

### Not Using Docker

You need a .NET Core 2.0+ toolchain. You can get one for Windows, Mac, or various Linuxes on Microsoft's website.

1. Have a mysql server
1. Restore NuGET packages `dotnet restore`
1. Set all the above configuration variables for docker either as environment variables or as user secrets using the .NET Core SecretManager.
1. Migrate the databse
1. Build and run `dotnet run`

If you want anything more complicated than that, you're on your own.

## Todos

This whole thing is a giant WIP. A Todo list will be forthcoming.

## License

![AGPL](https://www.gnu.org/graphics/agplv3-155x51.png)

This program is licensed under the terms of the GNU Affero General Public License, Version 3.

    Copyright (C) 2018  William M. Temple II

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
