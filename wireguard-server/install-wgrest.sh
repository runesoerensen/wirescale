#!/bin/bash

wgrest_version=1.0.0-alpha10.5

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
  curl -fssL https://github.com/suquant/wgrest/releases/download/${wgrest_version}/wgrest_$ARCH.deb -O
  dpkg -i wgrest_$ARCH.deb
}

install_wgrest
