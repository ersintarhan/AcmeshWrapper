#!/bin/bash
# Update version in AcmeshWrapper.csproj file

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Default project path
PROJECT_PATH="src/AcmeshWrapper/AcmeshWrapper.csproj"

# Function to validate semantic version
validate_version() {
    local version=$1
    # Semantic versioning regex
    if [[ $version =~ ^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(-((0|[1-9][0-9]*|[0-9]*[a-zA-Z-][0-9a-zA-Z-]*)(\.(0|[1-9][0-9]*|[0-9]*[a-zA-Z-][0-9a-zA-Z-]*))*))?(\+([0-9a-zA-Z-]+(\.[0-9a-zA-Z-]+)*))?$ ]]; then
        return 0
    else
        return 1
    fi
}

# Function to get version from git tag
get_git_tag_version() {
    # Check if we're on a tag
    if current_tag=$(git describe --exact-match --tags 2>/dev/null); then
        # Remove 'v' prefix if present
        version="${current_tag#v}"
        echo "$version"
        return 0
    fi
    
    # If not on a tag, show latest tag as info
    if latest_tag=$(git describe --tags --abbrev=0 2>/dev/null); then
        echo -e "${YELLOW}Not on a tag. Latest tag is: $latest_tag${NC}" >&2
    else
        echo -e "${YELLOW}No git tags found in repository${NC}" >&2
    fi
    
    return 1
}

# Function to update version in project file
update_version() {
    local version=$1
    local file=$2
    
    # Check if file exists
    if [ ! -f "$file" ]; then
        echo -e "${RED}Error: Project file not found at $file${NC}"
        return 1
    fi
    
    # Get current version
    current_version=$(grep -oP '(?<=<Version>)[^<]+(?=</Version>)' "$file" || echo "not found")
    echo -e "${YELLOW}Current version: $current_version${NC}"
    
    # Update version
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        sed -i '' "s|<Version>.*</Version>|<Version>$version</Version>|" "$file"
    else
        # Linux
        sed -i "s|<Version>.*</Version>|<Version>$version</Version>|" "$file"
    fi
    
    # Verify update
    new_version=$(grep -oP '(?<=<Version>)[^<]+(?=</Version>)' "$file" || echo "")
    if [ "$new_version" == "$version" ]; then
        echo -e "${GREEN}Successfully updated version to: $version${NC}"
        return 0
    else
        echo -e "${RED}Error: Version update verification failed${NC}"
        echo -e "${RED}Expected: $version, Found: $new_version${NC}"
        return 1
    fi
}

# Function to show usage
show_usage() {
    cat << EOF
Usage: $0 [OPTIONS] [VERSION]

Update version in AcmeshWrapper.csproj file.

OPTIONS:
    -p, --project PATH    Path to csproj file (default: $PROJECT_PATH)
    -h, --help           Show this help message

VERSION:
    Semantic version string (e.g., 1.2.3, 1.2.3-beta.1)
    If not provided, will attempt to get from current git tag

EXAMPLES:
    $0 1.2.3                    # Update to version 1.2.3
    $0                          # Get version from current git tag
    $0 -p path/to/file.csproj 1.2.3  # Update specific project file

EOF
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--project)
            PROJECT_PATH="$2"
            shift 2
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        -*)
            echo -e "${RED}Unknown option: $1${NC}"
            show_usage
            exit 1
            ;;
        *)
            VERSION="$1"
            shift
            ;;
    esac
done

# Main execution
main() {
    # Get version if not provided
    if [ -z "$VERSION" ]; then
        echo -e "${CYAN}No version specified. Attempting to get from git tag...${NC}"
        if VERSION=$(get_git_tag_version); then
            echo -e "${GREEN}Using version from git tag: $VERSION${NC}"
        else
            echo -e "${RED}Error: No version provided and could not determine version from git tag${NC}"
            exit 1
        fi
    fi
    
    # Validate version format
    if ! validate_version "$VERSION"; then
        echo -e "${RED}Error: Invalid version format: '$VERSION'${NC}"
        echo -e "${RED}Expected semantic versioning (e.g., 1.2.3, 1.2.3-beta.1)${NC}"
        exit 1
    fi
    
    # Find project file
    if [ ! -f "$PROJECT_PATH" ]; then
        # Try from script directory
        SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
        ALT_PATH="$(dirname "$SCRIPT_DIR")/$PROJECT_PATH"
        
        if [ -f "$ALT_PATH" ]; then
            PROJECT_PATH="$ALT_PATH"
        else
            echo -e "${RED}Error: Project file not found at: $PROJECT_PATH${NC}"
            exit 1
        fi
    fi
    
    echo -e "${CYAN}Updating version in: $PROJECT_PATH${NC}"
    
    # Update the version
    if update_version "$VERSION" "$PROJECT_PATH"; then
        exit 0
    else
        exit 1
    fi
}

# Run main function
main