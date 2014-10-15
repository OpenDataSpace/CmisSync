PACKAGE=dataspacesync
TBZ=$(top_builddir)/SOURCES/dataspacesync.tar.bz2
LIBDIR=.$(libdir)/dataspace-sync
SHARPDLLS=atk-sharp.dll gdk-sharp.dll glib-sharp.dll gtk-sharp.dll pango-sharp.dll
GLUELIBS=libatksharpglue-2.so libgdksharpglue-2.so libglibsharpglue-2.so libgtksharpglue-2.so libpangosharpglue-2.so
FATDLLS=CmisSync.Lib.dll DotCMIS.dll DBreeze.dll DotCMIS.dll log4net.dll Newtonsoft.Json.dll $(SHARPDLLS:%=./lib/%)
LIBPOSIXHELPER=libMonoPosixHelper.so

EASYDEPS:=$(shell dpkg -S "\-sharp.dll" | grep -E "/(notify|dbus)-sharp.dll$$" | cut -f2 -d ' ' | xargs -L 1 dirname | paste -s -d ':' -)
define findinst
$(shell dpkg -S $(1) | grep -e "/$(strip $(1))$$" | cut -f2 -d ' ')
endef


all:

dist: clean build
	test -f $(PACKAGE)*.changes && echo "Uploading $(PACKAGE)" && dput -c dput.cf $(PACKAGE)*.changes

$(GLUELIBS): 
	cd $(LIBDIR); \
	mkdir -p ./lib/; \
	cp $(call findinst, $@) ./lib/

$(LIBPOSIXHELPER):
	cd $(LIBDIR); \
	cp $(call findinst, $@) .

$(SHARPDLLS):
	cd $(LIBDIR); \
	mkdir -p ./lib/; \
	cp $(call findinst, $@) ./lib/; \
	conffile=$(call findinst, $@.config); \
	if [ -n "$$conffile" ]; then cp $$conffile ./lib/; fi

preparelibs: $(GLUELIBS) $(LIBPOSIXHELPER) $(SHARPDLLS)
	cd $(LIBDIR); \
	sed -i '/<dllmap.*sharpglue.*/d' lib/*.dll.config; \
	for curlib in $(GLUELIBS); do \
	  curlibname="$${curlib#lib}"; \
	  curlibname="$${curlibname%.*}"; \
	  sed -i "\$$i  <dllmap dll=\"$${curlibname}\" target=\".\/lib\/$${curlib}\"\/>" lib/*.dll.config; \
	done

fat: preparelibs
	tar xjf $(TBZ)
	cd $(LIBDIR) ; \
	export MONO_PATH="$(EASYDEPS)"; \
	mkbundle --static -o DataSpaceSync DataSpaceSync.exe --deps $(FATDLLS) --machine-config /etc/mono/4.5/machine.config -z ; \
	mkbundle --static -o DataSpaceSync.Console DataSpaceSync.Console.exe --deps $(FATDLLS) --machine-config /etc/mono/4.5/machine.config -z

build: fat
	cd $(PACKAGE) && dpkg-buildpackage -d -b -tc -us -uc

clean:
	rm -f *.deb *.changes *.upload
