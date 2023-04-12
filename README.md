# portable-mysql-8
This is a portable MySQL frontend to provide a quick and easy to use base to build an OpenSim install on.

It is intended to be a partial replacement for MOWES and Sim-on-a-Stick, both of which have not been maintained for a long time.

This is (currently) a Windows only project and does not contain a MySQL install or an OpenSim install. You will need to provide those files yourself and know how to set up OpenSim yourself. This software is only a frontend to make a lot of the painful MySQL stuff easier to set up, startup, shutdown, and provide ease of portability for those things.

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

### How To Use
**Note: I'll be using mysql-8.0.32-winx64.zip as an example for this how to**

1. Clone and build this repo or download and extract the latest release zip file of PortableMySQL8

2. Run PortableMySQL8.exe. It will create a new sub-directory named 'mysql' in the same directory as the exe. Inside the 'mysql' directory there will be a 'config' directory with a new default 'my.ini' config file created within; leave this alone for now. (Note: If it doesn't create the 'mysql' directory then close PortableMySQL8 and create the directory by hand in the same directory as PortableMySQL8.exe without the quotes. You don't have to create the config directory, it will be created automatically)

3. Close PortableMySQL8 if it is still open

4. Download the latest v8.0.x standalone MySQL Community Server from https://dev.mysql.com/downloads/mysql/ . You need the **"Windows (x86, 64-bit), ZIP Archive"** package from the **"Other Downloads"** section (***not*** the installer. In my case, this was mysql-8.0.32-winx64.zip)

5. Open the MySQL zip file you just downloaded and navigate into the mysql-8.0.32-winx64 folder (or whatever version number it is for you). You should see a few other things in here such as bin, docs, include, etc. All of this should be placed in the mysql directory made in PortableMySQL8.exe's directory.

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

6. Run PortableMySQL8.exe again. Enter a new password for the root MySQL user, set the port you want MySQL to run on (The default 3306 is fine if you have no other MySQL instances), and click the 'Start MySQL' button. It should do first time initialization for MySQL, set the password you chose a moment ago for the root user, and then the status should show "MySQL is running" in green text.

7. That's it! You can go to the Database tab and create your new opensim schemas from there (be sure to give it the proper user credentials, by default it's User: root, Server: localhost, and Password: <password you picked in step 6>), or use your favorite MySQL tool of choice to do that.



