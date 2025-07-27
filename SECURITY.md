# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.1.x   | :white_check_mark: |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

If you discover a security vulnerability within AcmeshWrapper, please:

1. **DO NOT** open a public issue
2. Send an email to the maintainer with details
3. Include steps to reproduce if possible
4. Wait for confirmation before disclosing publicly

## Security Best Practices

When using AcmeshWrapper:

1. **Protect Private Keys**: Never commit private keys or certificates to source control
2. **Secure API Tokens**: Store Cloudflare/DNS provider tokens securely
3. **File Permissions**: Ensure certificate files have appropriate permissions
4. **Use Environment Variables**: Store sensitive configuration in environment variables
5. **Regular Updates**: Keep both AcmeshWrapper and acme.sh updated

## Certificate Storage

AcmeshWrapper does not store certificates. It relies on acme.sh's storage mechanism. Ensure:

- Certificate directory has restricted permissions (e.g., 700)
- Private keys are readable only by necessary services
- Regular backups of certificate directory

## Dependencies

This project depends on:
- ProcessX - for process execution
- acme.sh - the underlying ACME client

Please ensure these dependencies are kept up to date.