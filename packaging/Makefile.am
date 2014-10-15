PACKAGE=dataspacesync
TBZ=$(top_builddir)/SOURCES/dataspacesync.tar.bz2
TARGET_LIBDIR=$(libdir)/dataspace-sync
LIBDIR=.$(TARGET_LIBDIR)

##################################################################
# Hi, welcome to the try of doing fatbuilds, I'll be your guide ;-)
#
# - Guide, what's happening?
# Well lad, we got trouble making the fat binary here, because the
# DLL's still want to load some shared libraries.
#
# - But which kind of libraries are these?
# You know, when some DLLs from the C# world want to talk to the
# Unix world, they need something that allows interaction.
# These are the 'gluelibs'.
# With these, guys from C# can talk to actual native libraries.
# E.g. buddy gtk-sharp will use libgtksharpglue to talk to gtk
# folks.
#
# - Oh man, I guess our binary won't be really fat then?
# Clever boy. We need to deploy these gluelibs along with the fatbinary.
# We will put them here:

DESTLIBDIR=lib

# - But how do we find out which glue libraries we need, do we have
#   a tool for that?
# Oh no! That would be too nice. I found this out by my own with try
# and error:

GLUELIBS=libatksharpglue-2.so libgdksharpglue-2.so libglibsharpglue-2.so libgtksharpglue-2.so libpangosharpglue-2.so libMonoPosixHelper.so

#
# - But how will mono know where these are on the target system?
# We need some DllMaps to do this.
#
# - Oh cool, so we simply write some configuration and that's it?
# Hahaha, silly boy, don't be naive. The world isn't that easy.
# 
# - So what do we need to do?
# We will copy all needed DLLs to a new location (also DESTLIBDIR),
# copy there config files to that same location and modify it.
# This is the only way mkbundle will find and use it.
#
# - Sounds pretty complicated. Can we find out which DLLs?
# It is. Not, not automatically, I set up a list:

SHARPDLLS=4.5/Mono.Posix.dll atk-sharp.dll gdk-sharp.dll glib-sharp.dll gtk-sharp.dll pango-sharp.dll

# We will then explicitly name then when calling mkbundle with our program DLLs

FATDLLS=CmisSync.Lib.dll DotCMIS.dll DBreeze.dll DotCMIS.dll log4net.dll Newtonsoft.Json.dll $(addprefix ./$(DESTLIBDIR)/, $(notdir $(SHARPDLLS)))

# - But how do we find the DLLs, configs and gluelibs on our system?
# Because they are on different locations on each system, this isn't easy.
# We will need to ask the package manager to find them for us

define findinst
$(shell dpkg -S $(1) | grep -e "/$(strip $(1))$$" | cut -f2 -d ' ')
endef

# For other DLLs without DLLImports we will only need there searchpath

EASYDEPS:=$(shell dpkg -S "\-sharp.dll" | grep -E "/(notify|dbus)-sharp.dll$$" | cut -f2 -d ' ' | xargs -L 1 dirname | paste -s -d ':' -)



all:

dist: clean build
	test -f $(PACKAGE)*.changes && echo "Uploading $(PACKAGE)" && dput -c dput.cf $(PACKAGE)*.changes

# Here is a target that simply finds and copies our gluelibs to our desired dir (DESTLIBDIR)
$(GLUELIBS): 
	cd $(LIBDIR); \
	mkdir -p ./$(DESTLIBDIR)/; \
	cp $(call findinst, $@) ./$(DESTLIBDIR)/

# This target will find the DLLs and the config files. If it doesn't find a config file (DllMap)
# it will simply create an empty one
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

# No look at this beaury, it removes all existing mappings for the gluelibs from the copied
# config files.
# Then it will add new mappings to all config files. It won't hurt if some config
# files have mappings they don't need. The target files will need an absolute path,
# so it's important to use the absolute path of our target (not the one used here)
preparelibs: $(GLUELIBS) $(SHARPDLLS)
	cd $(LIBDIR); \
	sed -i '/<dllmap.*sharpglue.*/d' ./$(DESTLIBDIR)/*.dll.config; \
	for curlib in $(GLUELIBS); do \
	  curlibname="$${curlib#lib}"; \
	  curlibname="$${curlibname%.*}"; \
	  sed -i "\$$i  <dllmap dll=\"$${curlibname}\" target=\"$(subst /,\/,$(TARGET_LIBDIR))\/$(DESTLIBDIR)\/$${curlib}\"\/>" $(DESTLIBDIR)/*.dll.config;  \
	done; 

# Now we can build the fat binaries. mkbundle will now be able to find alle the copied and modified configurations/mappings, because
# we tell it explicitly to use our copied DLLs (included in $FATDLLs), instead of letting it find them.
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
