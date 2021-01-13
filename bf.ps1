Param (
    #[Parameter]
    #[string[]] $args
    #[string] $args
)
docker run -v ${PWD}:/mnt/bfcli wilsoncg/bf-cli $args