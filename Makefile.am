ACLOCAL_AMFLAGS = -I build/m4/cmissync -I build/m4/shamrock

basedirs = build CmisSync.Lib DataSpaceSync.Console

SUBDIRS = $(basedirs) $(GUISUBDIRS)
DIST_SUBDIRS = $(basedirs) CmisSync

EXTRA_DIST = \
	README.md \
	legal/LICENSE.txt \
	legal/TRADEMARK.txt \
	legal/AUTHORS.txt \
	Extras/DotCMIS.dll \
	Extras/Newtonsoft.Json.dll \
	Extras/nunit.framework.dll

DISTCLEANFILES = \
	intltool-extract \
	intltool-merge \
	intltool-update \
	gnome-doc-utils.make

CLEANFILES = -r $(top_builddir)/bin

MAINTAINERCLEANFILES = \
	aclocal.m4 config.* configure install-sh ltmain* missing Makefile.in

bootstrap:
	autoreconf -f -s -i

debian-package:
	rm -rf $(top_builddir)/debian/usr
	make DESTDIR=$(abs_top_builddir)/debian install
	make VERSION=`git describe --match linux* | cut -d - -f 2` -C debian build

apple-package:
	"/Applications/Xamarin Studio.app/Contents/MacOS/mdtool" build CmisSync/Mac/CmisSync.sln -c:Release -t:Clean
	"/Applications/Xamarin Studio.app/Contents/MacOS/mdtool" build CmisSync/Mac/CmisSync.sln -c:Release
	(cd build; sh dmg-creator.sh ../bin/DataSpaceSync.app/; cd ..)

none:

entityframework: System.Data.SQLite.Linq.dll

System.Data.SQLite.dll:
	xbuild Extras/OpenDataSpace.System.Data.SQLite/System.Data.SQLite/System.Data.SQLite.2012.csproj /p:UseInteropDll=false /p:UseSqliteStandard=true /p:InteropLog=false /t:Rebuild
	cp -f Extras/OpenDataSpace.System.Data.SQLite/bin/2012/Debug/bin/System.Data.SQLite.* Extras

EntityFramework.dll:
	xbuild Extras/OpenDataSpace.entityframework/src/EntityFramework/EntityFramework.OpenDataSpace.mono.csproj /t:Rebuild
	cp -f Extras/OpenDataSpace.entityframework/src/EntityFramework/bin/Debug/EntityFramework.* Extras

System.Data.SQLite.Linq.dll: System.Data.SQLite.dll EntityFramework.dll
	xbuild Extras/OpenDataSpace.System.Data.SQLite.Linq/System.Data.SQLite.Linq.OpenDataSpace.mono.csproj /t:Rebuild
	cp -f Extras/OpenDataSpace.System.Data.SQLite.Linq/bin/Debug/System.Data.SQLite.Linq.dll* Extras

DotCMIS.dll:
	xbuild Extras/OpenDataSpaceDotCMIS/DotCMIS/DotCMIS.csproj /t:Rebuild
	cp -f Extras/OpenDataSpaceDotCMIS/DotCMIS/bin/Debug/DotCMIS.* Extras

dist-hook:
	make $(PACKAGE_TARGET)
