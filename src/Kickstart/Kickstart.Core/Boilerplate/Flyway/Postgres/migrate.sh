#!/bin/bash

docker run --rm -it \
       --network=host \
       -v $(pwd)/migrations:/migrations \
       registry.company.com/flyway:1.0.0 migrate -locations=filesystem:/migrations -user=postgres -password=my-secret-pw -url=jdbc:postgresql://localhost/##databasename##
