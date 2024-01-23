

$rgName = 'greedy_group'
$rg = Get-AzResourceGroup -name $rgName
$containerName = 'esdb-node1'
$environmentName = 'cae-greedy-eus-dev'
$stgName = 'stggreedyeusdev'

## Vnet is required to enable TCP port mapping

az network vnet create `
    -g $rgName `
    --location eastus `
    --name vnet-greedy-eus-dev `
    --address-prefixes 10.0.0.0/16 `
    --subnet-name snet-greedy-infra-eus-dev `
    --subnet-prefixes 10.0.0.0/23

## After ccreation, the VNET needs to be delegated to the app environment resource provider

az network vnet subnet update `
    --resource-group $rgName `
    --name snet-greedy-infra-eus-dev `
    --vnet-name vnet-greedy-eus-dev `
    --delegations Microsoft.App/environments

# Use this script if you need to remove the delegation, you will need to firs remove the infra subnet integration for this to work

az network vnet subnet update `
    --resource-group $rgName `
    --name snet-greedy-infra-eus-dev `
    --vnet-name vnet-greedy-eus-dev `
    --delegations Microsoft.App/environments `
    --remove delegations

## This finally provisions the container app environment. Replace the id with the sbnet id (full name "subscriptions/...")

az containerapp env create `
    -n $environmentName `
    -g $rgName `
    --location eastus `
    --infrastructure-subnet-resource-id '/subscriptions/7643973f-a34b-4ac2-91a9-4705285f14e6/resourceGroups/greedy_group/providers/Microsoft.Network/virtualNetworks/vnet-greedy-eus-dev/subnets/snet-greedy-infra-eus-dev'

## I'm using container app environment integration with azure files to get easy acccess to the generated certificate and the install it in the client machine. Use the access key from the storage account where you hav previously created a file share

az containerapp env storage set `
    -g $rgName `
    -n $environmentName `
    --storage-name $stgName `
    --access-mode 'ReadWrite' `
    --azure-file-account-key 'AccountKey' `
    --azure-file-account-name $stgName `
    --azure-file-share-name 'certs'

## Once the env has been created and configured you'll need to create the actual container

# If this is a new node, you'll need to download and install and trust the client (CA) certificate in your client's OS certificate store
az containerapp create `
    --name secure-node `
    --resource-group $rgName `
    --yaml secure-node.yml `
    --query properties.latestRevision

# Make sure you update the client certificate generated by the init container by downloading it from the file share, installing and trusting it in your client machine.