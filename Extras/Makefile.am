CLEANFILES = -r DotCMIS.* \
			 OpenDataSpaceDotCMIS/DotCMIS/bin \
			 OpenDataSpaceDotCMIS/DotCMIS/obj \
			 OpenDataSpaceDotCMIS/DotCMIS/UnitTestbin \
			 OpenDataSpaceDotCMIS/DotCMISUnitTest/obj

MAINTAINERCLEANFILES = Makefile.in

EXTRA_DIST = OpenDataSpaceDotCMIS \
			 dataspace-client-sharp

all-local:
	nuget restore OpenDataSpaceDotCMIS/DotCMIS.sln
	xbuild OpenDataSpaceDotCMIS/DotCMIS/DotCMIS.csproj /t:Rebuild
	cp -f OpenDataSpaceDotCMIS/DotCMIS/bin/Debug/DotCMIS.* .
	nuget restore dataspace-client-sharp/dataspace-client-sharp.sln
	xbuild dataspace-client-sharp/DataSpace/DataSpace.csproj /t:Rebuild
	cp -f dataspace-client-sharp/DataSpace/bin/Debug/DataSpace.* .
	nuget restore dataspace-client-sharp/DotCMIS/DotCMIS.sln
