#!/bin/bash
#
# This script converts the various icons from their SVG
# source format to PNG (for Unix), ICO (for Windows)
# and ICNS (for Mac). It uses the following tools:
#
# rsvg from http://librsvg.sourceforge.net/
# png2icns from http://icns.sourceforge.net/
# icoutil from http://www.nongnu.org/icoutils/
# inkscape from http://www.inkscape.org/
#
# On Fedora, these are available out of the box in the
# packages "librsvg2", "icoutils", "libicns-utils" and
# "inkscape".

SIZES="32 16 22 24 48 128 256 512"
mkdir -p scalable
SVGS=`find scalable -name "*.svg"`

function cnv() {
    SRC=$1
	[ -d ico ] || mkdir ico
    MACDST=ico/`basename $SRC .svg`.icns
    WINDST=ico/`basename $SRC .svg`.ico
    PNGALL=
    PNGWIN=
    for s in $SIZES ; do
        [ -d "${s}x${s}" ] || mkdir "${s}x${s}"
        PNG=`echo $SRC|sed -e "s/scalable/${s}x${s}/" -e 's/\.svg/.png/'`
        inkscape --export-png=$PNG --export-background-opacity=0 --export-width=$s --export-height=$s--without-gui $SRC  
        if  [ $s -ne 22 ] && [ $s -ne 24 ]; then
		PNGALL="$PNGALL $PNG"
	fi
        if [ $s -lt 512 ]; then
		PNGWIN="$PNGWIN $PNG"
        fi
    done
    png2icns $MACDST $PNGALL > /dev/null
    icotool -c -o $WINDST $PNGWIN
}

mkdir -p ico

for f in $SVGS ; do
    cnv $f
done
