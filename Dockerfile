# Base image of the docker container
FROM node:10-alpine

# Add your CLI's installation setups here using the RUN command
RUN npm install -g @microsoft/botframework-cli@4.10.1

ADD .docker/botframework-cli.config.json /root/.config/@microsoft/botframework-cli/config.json
RUN ["mkdir", "/mnt/bfcli"]

# Provdide a path to your cli apps executable
# /usr/local/bin/bf -> /usr/local/lib/node_modules/@microsoft/botframework-cli/bin/run
# CMD ["/bin/sh"]
ENTRYPOINT ["/usr/local/bin/bf"]
