FROM mcr.microsoft.com/mssql/server:2022-latest
USER root

RUN mkdir -p /usr/config
WORKDIR /usr/config

COPY . /usr/config

RUN chmod +x /usr/config/initialization.sh

ENTRYPOINT ["./entrypoint.sh"]