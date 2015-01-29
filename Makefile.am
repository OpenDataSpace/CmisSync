ACLOCAL_AMFLAGS = -I build/m4/cmissync -I build/m4/shamrock

basedirs = build Extras CmisSync.Lib DataSpaceSync.Console
FATBUILD_MAKEFILE=Makefile.fatbuild

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
	mkdir -p bin
	cp packages/NUnit.2.6.4/lib/nunit.framework.dll bin/nunit.framework.dll
	cp packages/Newtonsoft.Json.6.0.8/lib/net40/Newtonsoft.Json.dll bin/Newtonsoft.Json.dll
	cp packages/log4net.2.0.3/lib/net40-client/log4net.dll bin/log4net.dll
	cp packages/DBreeze.1.61.1/lib/net45/DBreeze.dll bin/DBreeze.dll

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

# make it a .PHONY target, so it's always executed
.PHONY: tmpinst
tmpinst:
	rm -rf tmpinst
	$(MAKE) DESTDIR=$(abs_top_builddir)/tmpinst install
	cp $(FATBUILD_MAKEFILE) tmpinst$(pkglibdir)/Makefile
	$(MAKE) LIBDIR=$(pkglibdir) PACKAGE_TARGET=$(PACKAGE_TARGET) -C tmpinst$(pkglibdir)
	rm tmpinst$(pkglibdir)/Makefile
	rm -rf SOURCES SRPMS SPECS RPMS BUILD BUILDROOT
	mkdir SOURCES
	tar cjCf tmpinst SOURCES/dataspacesync.tar.bz2 usr
	rm -rf tmpinst

debian-package: PACKAGE_TARGET := debian-package
debian-package: tmpinst
	make BASEDIR=$(abs_top_builddir)/packaging VERSION=$(VERSION) -C packaging build

apple-package:
	"/Applications/Xamarin Studio.app/Contents/MacOS/mdtool" build CmisSync/Mac/CmisSync.sln -c:Release -t:Clean
	"/Applications/Xamarin Studio.app/Contents/MacOS/mdtool" build CmisSync/Mac/CmisSync.sln -c:Release
	(cd build; sh dmg-creator.sh ../bin/DataSpaceSync.app/; cd ..)

rpm-package: PACKAGE_TARGET := rpm-package
rpm-package: tmpinst
	rm -f *.rpm
	rpmbuild -bb --define "_topdir $(abs_top_builddir)" --define "_libdir $(libdir)" build/cmissync.spec
	mv `find RPMS -name "*.rpm"` .
	rm -rf SOURCES SRPMS SPECS RPMS BUILD BUILDROOT

none:

package:
	make $(PACKAGE_TARGET)
