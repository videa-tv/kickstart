#!/bin/bash

docker run --rm -it \
       --network=host \
       registry.company.com/flyway:1.0.0 clean -user=root -password=my-secret-pw -url=jdbc:mysql://localhost/##databasename##
