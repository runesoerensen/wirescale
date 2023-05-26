#!/bin/bash

install_wgrest() {
  ARCH=""
  case $(uname -m) in
  x86_64)
    ARCH="amd64"
    ;;
  aarch64)
    ARCH="arm64"
    ;;
  **)
    echo "ERROR: The system architecture isn't supported"
    exit 1
    ;;
  esac
  curl -L https://github.com/suquant/wgrest/releases/latest/download/wgrest_$ARCH.deb -o wgrest_amd.deb
  dpkg -i wgrest_amd.deb
}

install_wgrest
