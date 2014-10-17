PACKAGE=dataspacesync
TBZ=$(top_builddir)/SOURCES/dataspacesync.tar.bz2

all:

dist: clean build
	test -f $(PACKAGE)*.changes && echo "Uploading $(PACKAGE)" && dput -c dput.cf $(PACKAGE)*.changes

build: 
	tar xjf $(TBZ)
	cd $(PACKAGE) && dpkg-buildpackage -d -b -tc -us -uc

clean:
	rm -f *.deb *.changes *.upload
