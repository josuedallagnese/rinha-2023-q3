#!/usr/bin/bash

cd ../
docker image rm banckend-web
docker build -t banckend-web .