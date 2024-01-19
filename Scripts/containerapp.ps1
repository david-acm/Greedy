

$rgName = 'greedy_group'
$rg = Get-AzResourceGroup -name $rgName
$containerName = 'esdb-node1'
$environmentName = 'cae-greedy-eus-dev'

az network vnet create `
    -g $rgName `
    --location eastus `
    --name vnet-greedy-eus-dev `
    --address-prefixes 10.0.0.0/16 `
    --subnet-name snet-greedy-infra-eus-dev `
    --subnet-prefixes 10.0.0.0/23

az network vnet subnet update `
    --resource-group $rgName `
    --name snet-greedy-infra-eus-dev `
    --vnet-name vnet-greedy-eus-dev `
    --delegations Microsoft.App/environments

az containerapp env create `
    -n $environmentName `
    -g $rgName `
    --location eastus `
    --infrastructure-subnet-resource-id /subscriptions/7643973f-a34b-4ac2-91a9-4705285f14e6/resourceGroups/greedy_group/providers/Microsoft.Network/virtualNetworks/vnet-greedy-eus-dev/subnets/snet-greedy-infra-eus-dev

az containerapp create `
    --name $containerName `
    --resource-group $rgName `
    --yaml containerapp.yaml `
    --query properties.configuration.ingress.fqdn

az containerapp compose create `
    --resource-group $rgName `
    --environment cae-greedy-dev-eus `
    --compose-file-path docker-compose.yaml


#  az containerapp up `
#  --name $containerName `
#  --resource-group $rgName `
#  --location $rg.Location `
#  --environment $environmentName `
#  --image eventstore/eventstore:latest `
#  --target-port 2113 1113 `
#  --env-vars `
#  --ingress external `
#  --query properties.configuration.ingress.fqdn
