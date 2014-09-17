PACKAGE=dataspacesync
TBZ=$(top_builddir)/SOURCES/dataspacesync.tar.bz2
LIBDIR=.$(libdir)/dataspace-sync
FATDLLS=CmisSync.Lib.dll DotCMIS.dll DBreeze.dll DotCMIS.dll log4net.dll Newtonsoft.Json.dll

all:

dist: clean build
	test -f $(PACKAGE)*.changes && echo "Uploading $(PACKAGE)" && dput -c dput.cf $(PACKAGE)*.changes

fat:
	tar xjf $(TBZ)
	cd $(LIBDIR) ; \
	if test -d /usr/lib/cli ; then \
		export MONO_PATH=`find /usr/lib/cli -name "*.dll" -printf '%h\n'|sort -u|perl -pe 's/\s/:/'` ; \
	fi ; \
	mkbundle --static -o DataSpaceSync DataSpaceSync.exe --deps $(FATDLLS) --machine-config /etc/mono/4.5/machine.config -z ; \
	mkbundle --static -o DataSpaceSync.Console DataSpaceSync.Console.exe --deps $(FATDLLS) --machine-config /etc/mono/4.5/machine.config -z

build: fat
	cd $(PACKAGE) && dpkg-buildpackage -d -b -tc -us -uc

clean:
	rm -f *.deb *.changes *.upload
