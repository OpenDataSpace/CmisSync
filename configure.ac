dnl Process this file with autoconf to produce a configure script.
m4_define([cmissync_version], [2.1.1])

AC_PREREQ([2.54])
AC_INIT([DataSpace Sync], cmissync_version)
AC_CONFIG_SRCDIR([CmisSync/Program.cs])
AC_CANONICAL_TARGET
AM_INIT_AUTOMAKE([foreign tar-ustar dist-bzip2 dist-zip no-dist-gzip])
AM_MAINTAINER_MODE

dnl Export Version Info
AC_SUBST([ASM_VERSION], [cmissync_version])

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR

AC_PROG_INSTALL

dnl Mono and gmcs
SHAMROCK_CHECK_MONO_MODULE(3.2)
SHAMROCK_FIND_MONO_RUNTIME
    
PKG_CHECK_EXISTS([mono >= 3.2],
[SHAMROCK_FIND_MONO_4_0_COMPILER
SHAMROCK_CHECK_MONO_4_0_GAC_ASSEMBLIES([
    System
    System.Configuration
    System.Security
    System.Windows.Forms
    System.Xml
    System.Xml.Linq
    System.Net
    System.Web
    System.Core
    System.Runtime.Serialization
    Mono.Posix
])]
)

AC_SUBST(NUNIT_LIBS,[[-r:\$\(top_srcdir\)/packages/NUnit.2.6.4/lib/nunit.framework.dll]])
AC_SUBST(NUNIT_PATH,[[\$\(top_srcdir\)/packages/NUnit.2.6.4/lib/nunit.framework.dll]])

AC_SUBST(JSON_LIBS,[[-r:\$\(top_srcdir\)/packages/Newtonsoft.Json.6.0.8/lib/net40/Newtonsoft.Json.dll]])
AC_SUBST(JSON_PATH,[[\$\(top_srcdir\)/packages/Newtonsoft.Json.6.0.8/lib/net40/Newtonsoft.Json.dll]])

AC_SUBST(LOG4NET_LIBS,[[-r:\$\(top_srcdir\)/packages/log4net.2.0.3/lib/net40-client/log4net.dll]])
AC_SUBST(LOG4NET_PATH,[[\$\(top_srcdir\)/packages/log4net.2.0.3/lib/net40-client/log4net.dll]])

AC_SUBST(DBREEZE_LIBS,[-r:\$\(top_srcdir\)/packages/DBreeze.1.71.0/lib/net40/DBreeze.dll])
AC_SUBST(DBREEZE_PATH,[\$\(top_srcdir\)/packages/DBreeze.1.71.0/lib/net40/DBreeze.dll])

AC_SUBST(MOQ_LIBS,[[-r:\$\(top_srcdir\)/packages/Moq.4.2.1409.1722/lib/net40/Moq.dll]])
AC_SUBST(MOQ_PATH,[[\$\(top_srcdir\)/packages/Moq.4.2.1409.1722/lib/net40/Moq.dll]])

AC_SUBST(ZEROMQ_LIBS,[[-r:\$\(top_srcdir\)/packages/ZeroMQ.4.1.0.15/lib/net40/ZeroMQ.dll]])
AC_SUBST(ZEROMQ_PATH,[[\$\(top_srcdir\)/packages/ZeroMQ.4.1.0.15/lib/net40/ZeroMQ.dll]])

AC_SUBST(DOTCMIS_LIBS,[[-r:\$\(top_srcdir\)/Extras/DotCMIS.dll]])
AC_SUBST(DOTCMIS_PATH,[[\$\(top_srcdir\)/Extras/DotCMIS.dll]])

CMISSYNC_CHECK_NOTIFY_SHARP
CMISSYNC_CHECK_GTK_SHARP

AC_ARG_ENABLE(appindicator,
	AS_HELP_STRING([--enable-appindicator[=@<:@no/auto/yes@:>@]],[Build support for application indicators ]),
	[enable_appindicator=$enableval],
	[enable_appindicator="auto"])

TEST_URL="https://devel.dataspace.cc/cmis/atom11"
AC_ARG_WITH([test-url],
    AS_HELP_STRING([--with-test-url=URL],[Specify server URL for testing]),
    TEST_URL=${withval})
AC_SUBST(TEST_URL)

TEST_LOCALPATH="<localpath>"
AC_ARG_WITH([test-localpath],
    AS_HELP_STRING([--with-test-localpath=PATH],[Specify local path for testing]),
    TEST_LOCALPATH=${withval})
AC_SUBST(TEST_LOCALPATH)

TEST_REMOTEPATH="<remotepath>"
AC_ARG_WITH([test-remotepath],
    AS_HELP_STRING([--with-test-remotepath=PATH],[Specify remote path for testing]),
    TEST_REMOTEPATH=${withval})
AC_SUBST(TEST_REMOTEPATH)

TEST_USER="<username>"
AC_ARG_WITH([test-user],
    AS_HELP_STRING([--with-test-user=PATH],[Specify username for testing]),
    TEST_USER=${withval})
AC_SUBST(TEST_USER)

TEST_PASSWORD="<password>"
AC_ARG_WITH([test-password],
    AS_HELP_STRING([--with-test-password=PATH],[Specify password for testing]),
    TEST_PASSWORD=${withval})
AC_SUBST(TEST_PASSWORD)

TEST_REPOID="<repo id>"
AC_ARG_WITH([test-repoid],
    AS_HELP_STRING([--with-test-repoid=PATH],[Specify remote repository-ID for testing]),
    TEST_REPOID=${withval})
AC_SUBST(TEST_REPOID)

TEST_BINDING="atompub"
AC_ARG_WITH([test-binding],
    AS_HELP_STRING([--with-test-binding=BINDING],[Specify CMIS Binding for testing]),
    TEST_BINDING=${withval})
AC_SUBST(TEST_BINDING)

TEST_PROXY_USER=""
AC_ARG_WITH([test-proxy-user],
    AS_HELP_STRING([--with-test-proxy-user=PATH],[Specify user for authetication required proxy for testing]),
    TEST_PROXY_USER=${withval})
AC_SUBST(TEST_PROXY_USER)

TEST_PROXY_PASSWORD=""
AC_ARG_WITH([test-proxy-password],
    AS_HELP_STRING([--with-test-proxy-password=PATH],[Specify password for authetication required proxy for testing]),
    TEST_PROXY_PASSWORD=${withval})
AC_SUBST(TEST_PROXY_PASSWORD)

TEST_PROXY_URL=""
AC_ARG_WITH([test-proxy-url],
    AS_HELP_STRING([--with-test-proxy-url=PATH],[Specify proxy url for testing]),
    TEST_PROXY_URL=${withval})
AC_SUBST(TEST_PROXY_URL)


OSNAME=`uname -s`
OSOK=no

PACKAGE_TARGET=none

if test "$OSNAME" = "Linux" ; then

	dnl check for notify-sharp
	PKG_CHECK_MODULES(NOTIFY_SHARP, notify-sharp, have_notify_sharp=yes, have_notify_sharp=no)
	if test "x$have_notify_sharp" = "xno" ; then
		AC_ERROR([notify-sharp is a required dependency: you need to install the appropriate devel package before you can compile])
	fi
	AC_SUBST(NOTIFY_SHARP_LIBS)

	dnl check for appindicator
	APPINDICATOR_REQUIRED=0.0.7
	if test x$enable_appindicator = xauto ; then
			PKG_CHECK_EXISTS([appindicator-sharp-0.1 >= $APPINDICATOR_REQUIRED],
							 enable_appindicator="yes",
							 enable_appindicator="no")
	fi

	if test x$enable_appindicator = xyes ; then
			PKG_CHECK_EXISTS([appindicator-sharp-0.1 >= $APPINDICATOR_REQUIRED],,
							 AC_MSG_ERROR([appindicator-sharp-0.1 is not installed]))
			PKG_CHECK_MODULES(APP_INDICATOR,
							appindicator-sharp-0.1 >= $APPINDICATOR_REQUIRED)
			AC_SUBST(APP_INDICATOR_CFLAGS)
			AC_SUBST(APP_INDICATOR_LIBS)
            [BUILD_DEFINES="$BUILD_DEFINES -define:HAVE_APP_INDICATOR"]
	fi

    AC_SUBST(CONFIGLIBS,[-r:System.Configuration.dll])
    AC_SUBST(WEBLIBS,[-r:System.Web.dll])
    AC_SUBST(MONOPOSIX,[-r:Mono.Posix.dll])

    AC_SUBST(BUILD_DEFINES,"${BUILD_DEFINES}")
    GUISUBDIRS=CmisSync
    enable_gtkui=yes
	cocoa=false
	macos=false
    OSOK=yes

    # On Debian and xUbuntu, build debian packages, otherwise rpms. Don't know a better syntax for this
    if test `command -v lsb_release >/dev/null 2>&1 && lsb_release -i | grep -E "(ebian|buntu)" > /dev/null; echo "$?"` == "0"; then
        PACKAGE_TARGET=debian-package
    else
	PACKAGE_TARGET=rpm-package
    fi
fi

if test "$OSNAME" = "Darwin" ; then
	AC_MSG_RESULT([Building on OSX])

	AC_SUBST(NOTIFY_SHARP_LIBS)


    AC_SUBST(CONFIGLIBS,[-r:System.Configuration.dll])
    AC_SUBST(WEBLIBS,[-r:System.Web.dll])
    AC_SUBST(MONOPOSIX,[-r:Mono.Posix.dll])
    AC_SUBST(DRAWING, [-r:System.Drawing.dll])

    AC_ARG_WITH([monomac],
        AS_HELP_STRING([--with-monomac=PATH],[Specify path to MonoMac.dll]),
            [monomac_dll=${withval}],[monomac_dll=no])
    if test "x$monomac_dll" = "xno" ; then
        PKG_CHECK_MODULES(MONOMAC, xammac, have_xammac=yes, have_xammac=no)
        if test "x$have_xammac" = "xno" ; then
            AM_CONDITIONAL([HAVE_XAMMAC], false)
            PKG_CHECK_MODULES(MONOMAC, monomac, have_monomac=yes, have_monomac=no)
            if test "x$have_monomac" = "xno" ; then
                AM_CONDITIONAL([HAVE_MONOMAC], false)
                AC_ERROR([xammac or monomac are required dependency: you need to install the appropriate lib before you can compile])
                else
                AM_CONDITIONAL([HAVE_MONOMAC], true)
		AC_MSG_RESULT([HAVE_MONOMAC=true])
            fi
        else
        AM_CONDITIONAL([HAVE_XAMMAC], true)
        AM_CONDITIONAL([HAVE_MONOMAC], false)
	AC_MSG_RESULT([HAVE_MONOMAC=false])
        fi
    else
        AM_CONDITIONAL([HAVE_XAMMAC], false)
        AM_CONDITIONAL([HAVE_MONOMAC], true)
        AC_MSG_RESULT([HAVE_MONOMAC=true])
        if test -f ${monomac_dll} ; then
            case ${monomac_dll} in
                /*)
                AC_SUBST(MONOMAC_LIBS,[-r:${monomac_dll}])
                AC_SUBST(MONOMAC_PATH,[${monomac_dll}])
                ;;
                *)
                AC_SUBST(MONOMAC_LIBS,[[-r:\$\(top_srcdir\)/${monomac_dll}]])
                AC_SUBST(MONOMAC_PATH,[[\$\(top_srcdir\)/${monomac_dll}]])
                ;;
            esac
        else
            AC_ERROR([${monomac_dll} not found])
        fi
    fi
    AC_SUBST(MONOMAC_LIBS)

    AC_SUBST(BUILD_DEFINES,"${BUILD_DEFINES} -define:__COCOA__")
    GUISUBDIRS=CmisSync
    enable_gtkui=no
	cocoa=true
	macos=true
    OSOK=yes
    PACKAGE_TARGET=apple-package
else
    AM_CONDITIONAL([HAVE_XAMMAC], false)
    AM_CONDITIONAL([HAVE_MONOMAC], false)
fi

AC_SUBST(PACKAGE_TARGET)

AM_CONDITIONAL([HAVE_MAC_OS],[test x$macos = xtrue])
AM_CONDITIONAL([__COCOA__], [test x$cocoa = xtrue])

if test "$OSOK" = "no" ; then
	GUISUBDIRS=
    enable_gtkui=no
fi

AC_SUBST([GUISUBDIRS])

AC_OUTPUT([
build/cmissync.spec
build/Makefile
build/m4/Makefile
build/dmg-creator.sh
CmisSync.Lib/Defines.cs
CmisSync.Lib/Makefile
CmisSync/Common/Makefile
CmisSync/Common/HTML/Makefile
CmisSync/Common/Plugins/Makefile
CmisSync/Common/Pixmaps/Makefile
CmisSync/Linux/Defines.cs
CmisSync/Linux/Makefile
CmisSync/Linux/Pixmaps/Makefile
CmisSync/Linux/Pixmaps/icons/Makefile
CmisSync/Linux/Pixmaps/icons/ubuntu-mono-dark/Makefile
CmisSync/Linux/Pixmaps/icons/ubuntu-mono-light/Makefile
CmisSync/Mac/Makefile
CmisSync/Mac/Info.plist
CmisSync/Mac/Pixmaps/Makefile
CmisSync/TestLibrary/Makefile
CmisSync/TestLibrary/test-servers.json
CmisSync/TestLibrary/proxy-server.json
CmisSync/Linux/dataspacesync
DataSpaceSync.Console/Makefile
DataSpaceSync.Console/dsscli
DiagnoseTool/Makefile
DiagnoseTool/dssdiagnose
DiagnoseTool/Mac/DiagnoseTool/Info.plist
packaging/Makefile
packaging/dataspacesync/debian/changelog
CmisSync/Makefile
Extras/Makefile
Makefile
])

echo "
DataSpace Sync ${VERSION}

Configuration:
       Prefix              : ${prefix}
       Build Gtk+ UI       : ${enable_gtkui}

"
