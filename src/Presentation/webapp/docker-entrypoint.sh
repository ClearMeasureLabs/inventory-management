#!/bin/sh
set -e

# Replace the API URL in the Angular app's main.js file
# This allows runtime configuration of the API endpoint
if [ -n "$API_URL" ]; then
    echo "Configuring API URL: $API_URL"
    # Find the main-*.js file and replace the API URL placeholder
    for file in /usr/share/nginx/html/main-*.js; do
        if [ -f "$file" ]; then
            # Replace empty apiUrl with the provided API_URL
            sed -i "s|apiUrl:\"\"|apiUrl:\"$API_URL\"|g" "$file"
            echo "Updated $file with API URL"
        fi
    done
fi

# Start nginx
exec nginx -g 'daemon off;'
