PACKAGE=dataspacesync
TBZ=$(top_builddir)/SOURCES/dataspacesync.tar.bz2
LIBDIR=.$(libdir)/dataspace-sync
SHARPDLLS=atk-sharp.dll gdk-sharp.dll glib-sharp.dll gtk-sharp.dll pango-sharp.dll
GLUELIBS=libatksharpglue-2.so libgdksharpglue-2.so libglibsharpglue-2.so libgtksharpglue-2.so libpangosharpglue-2.so
EASYDEPS=dbus-sharp|notify-sharp
FATDLLS=CmisSync.Lib.dll DotCMIS.dll DBreeze.dll DotCMIS.dll log4net.dll Newtonsoft.Json.dll $(SHARPDLLS)
LIBPOSIXHELPER=libMonoPosixHelper.so
COPYDEPS=$(SHARPDLLS) $(GLUELIBS) $(LIBPOSIXHELPER)

all:

dist: clean build
	test -f $(PACKAGE)*.changes && echo "Uploading $(PACKAGE)" && dput -c dput.cf $(PACKAGE)*.changes

$(COPYDEPS): 
	pushd $(LIBDIR); \
	cp $(shell dpkg -S $@ | grep -e "/$@$$" | cut -f2 -d ' ') .; \
	popd

fat: $(COPYDEPS)
	tar xjf $(TBZ)
	cd $(LIBDIR) ; \
	export MONO_PATH=`dpkg -S "\-sharp.dll" | grep -E "/($(EASYDEPS)).dll$$" | cut -f2 -d ' ' | xargs -L 1 dirname | paste -s -d ':' -`; \
	mkbundle --static -o DataSpaceSync DataSpaceSync.exe --deps $(FATDLLS) --machine-config /etc/mono/4.5/machine.config -z ; \
	mkbundle --static -o DataSpaceSync.Console DataSpaceSync.Console.exe --deps $(FATDLLS) --machine-config /etc/mono/4.5/machine.config -z

build: fat
	cd $(PACKAGE) && dpkg-buildpackage -d -b -tc -us -uc

clean:
	rm -f *.deb *.changes *.upload
