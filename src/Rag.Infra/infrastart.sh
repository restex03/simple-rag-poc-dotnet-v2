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