#!/usr/bin/env bash
set -euo pipefail

if docker container inspect qdrant >/dev/null 2>&1; then
  docker start qdrant
else 
    docker run -d \
    --name qdrant \
    -p 6333:6333 \
    -p 6334:6334 \
    -v qdrant-storage:/qdrant/storage \
    qdrant/qdrant
fi



if docker container inspect opensearch >/dev/null 2>&1; then
  docker start opensearch
else 
docker run -d \
  --name opensearch \
  -p 9200:9200 \
  -p 9600:9600 \
  -e "discovery.type=single-node" \
  -e "DISABLE_SECURITY_PLUGIN=true" \
  opensearchproject/opensearch:latest
fi