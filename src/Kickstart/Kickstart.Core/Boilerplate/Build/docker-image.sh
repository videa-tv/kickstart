#!/bin/bash

build-docker-image() {
	publishDir="$1"
	imageName="$2"
	canpush="$3"

	docker build $publishDir -t $imageName

	# push image
	if [ "$canpush" == "true" ]; then
		docker push $imageName
	else
		echo "Docker push is disabled. Please pass 'true' to enable it"
	fi

	# cleanup
	docker rmi $imageName
}

bye() {
	  result=$?
	  if [ "$result" != "0" ]; then
		    echo "Build failed"
	  fi
	  exit $result
}

#Stop execution on any error
trap "bye" EXIT

set -e
