# Branch Protection Rules

## Current Protection (Basic)

Main branch is protected with:
- ❌ No force pushes
- ❌ No branch deletion  
- ✅ Status checks must pass (build)
- ✅ Branch must be up to date before merge

## Recommended for Team Projects

If working with a team, consider enabling:

```bash
# Require PR reviews (1 reviewer)
gh api -X PATCH repos/ersintarhan/AcmeshWrapper/branches/main/protection \
  --field "required_pull_request_reviews[required_approving_review_count]=1"

# Require all conversations resolved
gh api -X PATCH repos/ersintarhan/AcmeshWrapper/branches/main/protection \
  --field "required_conversation_resolution=true"

# Dismiss stale reviews when new commits pushed
gh api -X PATCH repos/ersintarhan/AcmeshWrapper/branches/main/protection \
  --field "required_pull_request_reviews[dismiss_stale_reviews]=true"
```

## For Solo Projects

Current settings are appropriate for solo projects where you:
- Want protection against accidental force pushes
- Need CI/CD checks to pass
- Don't need PR reviews for every change

## Bypass Protection (Emergency)

As admin, you can still:
1. Temporarily disable protection
2. Make emergency fixes
3. Re-enable protection

```bash
# Temporarily disable
gh api -X DELETE repos/ersintarhan/AcmeshWrapper/branches/main/protection

# Re-enable
gh api -X PUT repos/ersintarhan/AcmeshWrapper/branches/main/protection < protection-rules.json
```