#!/bin/bash
set -euxo pipefail

npm run build
cdk deploy --profile chessdb-prod CoreStack-prod --require-approval never
cdk deploy --profile chessdb-prod AuthStack-prod --require-approval never
cdk deploy --profile chessdb-prod StorageStack-prod --require-approval never
cdk deploy --profile chessdb-prod WebsiteStack-prod --require-approval never
cdk deploy --profile chessdb-prod ApiStack-prod --require-approval never