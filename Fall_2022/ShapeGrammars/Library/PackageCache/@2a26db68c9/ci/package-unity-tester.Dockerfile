
# Possible alterations of UNITY_VERSION argument, using example 2018.3.11f1:
# 2018.3.11f1				: all modules
# 2018.3.11f1-windows		: Windows module
# 2018.3.11f1-mac			: Mac OS X module
# 2018.3.11f1-ios			: iOS module
# 2018.3.11f1-android		: Android module
# 2018.3.11f1-webgl			: WebGL module
# 2018.3.11f1-facebook		: Facebook module
# 2018.3.11f1-linux-il2cpp	: Linux with IL2CPP support for Editor
ARG UNITY_VERSION=2019.2.11f1
FROM gableroux/unity3d:${UNITY_VERSION}

# Should correspond to the image tag
ARG IMAGE_VERSION
ENV IMAGE_VERSION=${IMAGE_VERSION}

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        jq=1.5+dfsg-2 \
        xmlstarlet=1.6.1-2 \
    # Cleanup cache
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /root/repo
