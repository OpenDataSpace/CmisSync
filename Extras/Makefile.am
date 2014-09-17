CLEANFILES = -r DotCMIS.* \
			 OpenDataSpaceDotCMIS/DotCMIS/bin \
			 OpenDataSpaceDotCMIS/DotCMIS/obj \
			 OpenDataSpaceDotCMIS/DotCMIS/UnitTestbin \
			 OpenDataSpaceDotCMIS/DotCMISUnitTest/obj

MAINTAINERCLEANFILES = Makefile.in

EXTRA_DIST = OpenDataSpaceDotCMIS

all-local:
	xbuild OpenDataSpaceDotCMIS/DotCMIS/DotCMIS.csproj /t:Rebuild
	cp -f OpenDataSpaceDotCMIS/DotCMIS/bin/Debug/DotCMIS.* .
	
