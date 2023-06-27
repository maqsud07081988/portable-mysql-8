# portable-mysql-8
This is a portable MySQL frontend to provide a quick and easy to use base to build an OpenSim install on.

It is intended to be a partial replacement for MOWES and Sim-on-a-Stick, both of which have not been maintained for a long time.

This is (currently) a **Windows only** project and does not contain a MySQL install or an OpenSim install. You will need to provide those files yourself and know how to set up OpenSim yourself. This software is only a frontend to make a lot of the painful MySQL stuff easier to set up, startup, shutdown, and provide ease of portability for those things.

Right now this project should be considered **alpha software**. Bugs, issues, quirks, and even unimplemented features should be expected. While I have taken care to reasonably test this to make sure there are no major issues; I am just one person and can not account for every situation this sofware could be ran under.

### What this project is intended for
* ***OpenSim specific*** MySQL use
* Quick and easy portability of your OpenSim install and database on your medium of choice
* Small, local, or personal grids where all services are on the same system

### What this project is ***NOT*** intended for
* Commercial use
* Government use
* Large scale use
* General purpose MySQL use not related to OpenSim
* Storage of data where security practices are paramount
* Any use where failure of the software could result in bodily harm and/or financial damage

**See [LICENSE.txt](./LICENSE.txt) for the license agreement.**

**See [ADDITIONAL_COPYRIGHTS.txt](./ADDITIONAL_COPYRIGHTS.txt) for a list of copyright holders of libraries used in this project.**

### Compile Requirements
* Visual Studio 2019 or higher with .NET desktop development option
* .NET 4.7.2 targeting pack (May change in the future)
* Nuget (Should have been automatically installed with VS; if not, install it via the VS installer tool)

### How To Use
**Note: I'll be using mysql-8.0.32-winx64.zip as an example for this how to**

1. Download and extract the latest release zip file of PortableMySQL8 or clone and build this repo

2. Run PortableMySQL8.exe and it should create a new sub-directory named 'mysql' in the same directory as the exe

4. Download the latest v8.0.x standalone MySQL Community Server from https://dev.mysql.com/downloads/mysql/
    - You need the **"Windows (x86, 64-bit), ZIP Archive"** package from the **"Other Downloads"** section, ***NOT*** the installer

5. Open the MySQL zip file you just downloaded and navigate into the mysql-8.0.32-winx64 folder (or whatever version number it is for you)
    - You should see a few things in here such as bin, docs, include, etc. ***All of this*** should be placed in the mysql directory made in PortableMySQL8.exe's directory.

When done you should have a directory structure that looks a bit like this:

- PortableMySQL8.exe directory
  - mysql
    - bin
    - config
    - docs
    - include
    - (...etc)

**If you ended up with this:**

- PortableMySQL8.exe directory
  - mysql
    - mysql-8.0.32-winx64 ***<-- Wrong***
      - bin
      - docs
      - (...etc)

**... then you did it wrong.**

5. Enter a new password for the root MySQL user and set the port you want MySQL to run on
    - The default port 3306 is fine if you have no other MySQL instances

7. Click the 'Start MySQL' button
    - It should do first time initialization for MySQL, set the root password, and then the status should show 'MySQL is running'
        - **The initialize step can take a few minutes and the program will appear frozen until it is done**
    - Initialization only needs to be done once (unless the data directory is moved or deleted)

8. That's it! You can go to the Database tab and create your opensim schemas as well as the MySQL user associated with those schemas from there (be sure root password and port is set properly on Main tab)... or you can use your favorite MySQL admin tool of choice to do that, the Database tab is simply for convenience.



