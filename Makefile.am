ACLOCAL_AMFLAGS = -I build/m4/cmissync -I build/m4/shamrock

basedirs = build Extras CmisSync.Lib DataSpaceSync.Console

SUBDIRS = $(basedirs) $(GUISUBDIRS)
DIST_SUBDIRS = $(basedirs) CmisSync

EXTRA_DIST = \
	README.md \
	legal \
	debian \
	packages

DISTCLEANFILES = \
	intltool-extract \
	intltool-merge \
	intltool-update \
	gnome-doc-utils.make

CLEANFILES = -r $(top_builddir)/bin SOURCES SRPMS SPECS RPMS BUILD BUILDROOT tmpinst

MAINTAINERCLEANFILES = \
	aclocal.m4 config.* configure install-sh ltmain* missing Makefile.in

bootstrap:
	autoreconf -f -s -i
	nuget restore CmisSync/Linux/CmisSync.sln

install-exec-local:
	$(INSTALL) -m 0755 $(JSON_PATH) $(DESTDIR)$(pkglibdir)
	$(INSTALL) -m 0755 $(NUNIT_PATH) $(DESTDIR)$(pkglibdir)
	$(INSTALL) -m 0755 $(MOQ_PATH) $(DESTDIR)$(pkglibdir)
	$(INSTALL) -m 0755 $(DBREEZE_PATH) $(DESTDIR)$(pkglibdir)
	$(INSTALL) -m 0755 $(LOG4NET_PATH) $(DESTDIR)$(pkglibdir)
	$(INSTALL) -m 0755 $(DOTCMIS_PATH) $(DESTDIR)$(pkglibdir)

uninstall-local:
	$(RM) \
		$(DESTDIR)$(pkglibdir)/$(notdir $(JSON_PATH)) \
		$(DESTDIR)$(pkglibdir)/$(notdir $(NUNIT_PATH)) \
		$(DESTDIR)$(pkglibdir)/$(notdir $(MOQ_PATH)) \
		$(DESTDIR)$(pkglibdir)/$(notdir $(DBREEZE_PATH)) \
		$(DESTDIR)$(pkglibdir)/$(notdir $(LOG4NET_PATH)) \
		$(DESTDIR)$(pkglibdir)/$(notdir $(DOTCMIS_PATH))

tmpinst:
	rm -rf tmpinst
	$(MAKE) DESTDIR=$(abs_top_builddir)/tmpinst install
	rm -rf SOURCES SRPMS SPECS RPMS BUILD BUILDROOT
	mkdir SOURCES
	tar cjCf tmpinst SOURCES/dataspacesync.tar.bz2 usr
	rm -rf tmpinst

debian-package: tmpinst
	make BASEDIR=$(abs_top_builddir)/debian VERSION=`git describe --match linux* | cut -d - -f 2` -C debian build

apple-package:
	"/Applications/Xamarin Studio.app/Contents/MacOS/mdtool" build CmisSync/Mac/CmisSync.sln -c:Release -t:Clean
	"/Applications/Xamarin Studio.app/Contents/MacOS/mdtool" build CmisSync/Mac/CmisSync.sln -c:Release
	(cd build; sh dmg-creator.sh ../bin/DataSpaceSync.app/; cd ..)

rpm-package: tmpinst
	rm -f *.rpm
	rpmbuild -bb --define "_topdir $(abs_top_builddir)" --define "_libdir $(libdir)" build/cmissync.spec
	mv `find RPMS -name "*.rpm"` .
	rm -rf SOURCES SRPMS SPECS RPMS BUILD BUILDROOT

none:

package:
	make $(PACKAGE_TARGET)
