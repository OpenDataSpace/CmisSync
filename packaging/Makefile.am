PACKAGE=dataspacesync
TBZ=$(top_builddir)/SOURCES/dataspacesync.tar.bz2
TARGET_LIBDIR=$(libdir)/dataspace-sync
LIBDIR=.$(TARGET_LIBDIR)
LIBPOSIXHELPER=libMonoPosixHelper.so
SHARPDLLS=4.5/Mono.Posix.dll atk-sharp.dll gdk-sharp.dll glib-sharp.dll gtk-sharp.dll pango-sharp.dll 
GLUELIBS=libatksharpglue-2.so libgdksharpglue-2.so libglibsharpglue-2.so libgtksharpglue-2.so libpangosharpglue-2.so $(LIBPOSIXHELPER)
DESTLIBDIR=lib
FATDLLS=CmisSync.Lib.dll DotCMIS.dll DBreeze.dll DotCMIS.dll log4net.dll Newtonsoft.Json.dll $(addprefix ./$(DESTLIBDIR)/, $(notdir $(SHARPDLLS)))


EASYDEPS:=$(shell dpkg -S "\-sharp.dll" | grep -E "/(notify|dbus)-sharp.dll$$" | cut -f2 -d ' ' | xargs -L 1 dirname | paste -s -d ':' -)
define findinst
$(shell dpkg -S $(1) | grep -e "/$(strip $(1))$$" | cut -f2 -d ' ')
endef


all:

dist: clean build
	test -f $(PACKAGE)*.changes && echo "Uploading $(PACKAGE)" && dput -c dput.cf $(PACKAGE)*.changes

$(GLUELIBS): 
	cd $(LIBDIR); \
	mkdir -p ./$(DESTLIBDIR)/; \
	cp $(call findinst, $@) ./$(DESTLIBDIR)/

$(SHARPDLLS):
	cd $(LIBDIR); \
	mkdir -p ./$(DESTLIBDIR)/; \
	cp $(call findinst, $@) ./$(DESTLIBDIR)/; \
	conffile=$(call findinst, $@.config); \
	if [ -n "$$conffile" ]; then \
	  cp $$conffile ./$(DESTLIBDIR)/; \
	else \
	  echo -e "<configuration>\n</configuration>\n" > ./$(DESTLIBDIR)/$(shell basename $@).config; \
        fi

preparelibs: $(GLUELIBS) $(SHARPDLLS)
	cd $(LIBDIR); \
	sed -i '/<dllmap.*sharpglue.*/d' ./$(DESTLIBDIR)/*.dll.config; \
	for curlib in $(GLUELIBS); do \
	  curlibname="$${curlib#lib}"; \
	  curlibname="$${curlibname%.*}"; \
	  sed -i "\$$i  <dllmap dll=\"$${curlibname}\" target=\"$(subst /,\/,$(TARGET_LIBDIR))\/$(DESTLIBDIR)\/$${curlib}\"\/>" $(DESTLIBDIR)/*.dll.config;  \
	done; 

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
