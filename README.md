## DataSpace Sync

[DataSpace Sync](http://graudata.com) allows you to keep in sync with your DataSpace (CMIS) repository.
See: http://graudata.com

### License

DataSpace Sync is Open Source software and licensed under the `GNU General Public License version 3 or later`. You are welcome to change and redistribute it under certain conditions. For more information see the `legal/LICENSE` file.

### Development

We are looking for volunteers!
See [how to get started developing DataSpace Sync or CmisSync](https://github.com/nicolas-raoul/CmisSync/wiki/Getting-started-with-CmisSync-development)

### Integration Testing

$ git submodule init
$ git submodule update
$ make -f Makefile.am
$ ./configure --with-test-url=http://localhost:8080/cmis/atom11 --with-test-binding=atompub --with-test-repoid=0a03fd20-689b-11e3-942b-5254008eefc5 --with-test-remotepath=/tmp --with-test-localpath=$HOME/tmp --with-test-user=jenkins --with-test-password=********
$ make
$ cp -a Extras/DotCMIS.dll bin
$ cp -a Extras/DotCMIS.dll.mdb bin
$ ./nunit-console -labels -run=TestLibrary.IntegrationTests.PrivateWorkingCopyTests.CreateDocumentTests.CreateCheckedOutDocumentMustFailIfDocumentAlreadyExists bin/TestLibrary.dll
