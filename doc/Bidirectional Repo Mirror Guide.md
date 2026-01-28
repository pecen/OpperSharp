# Bidirectional Repository Mirroring: Azure DevOps ↔ GitHub

## Complete Step-by-Step Guide

This guide will help you set up automatic two-way synchronization between an Azure DevOps repository and a GitHub repository. When you push to either one, the other will automatically update.

---

## Prerequisites

Before starting, make sure you have:

- An existing repository in Azure DevOps OR GitHub (one side must have code, the other must be empty)
- Accounts on both GitHub and Azure DevOps
- A web browser
- Git installed on your computer (for the initial setup)

---

## Which Direction Are You Starting From?

This guide supports two scenarios:

**Scenario A: You have an existing Azure DevOps repo** → Follow Parts 1-4 as written (create empty GitHub repo, mirror from Azure DevOps to GitHub)

**Scenario B: You have an existing GitHub repo** → Follow Part 1B below, then skip to Part 2

---

## Part 1B: Alternative — Starting with an Existing GitHub Repo

If you already have a GitHub repository that you want to mirror to Azure DevOps, follow these steps instead of Parts 1 and 4.

### Step 1B.1: Create an Empty Azure DevOps Repository

1. Go to **https://dev.azure.com**
2. Navigate to your organization and project
3. Click **Repos** in the left sidebar
4. If you have existing repos, click the dropdown at the top and select **+ New repository**
5. Fill in the details:
   - **Repository name**: Use the same name as your GitHub repo (recommended)
   - **⚠️ IMPORTANT**: Uncheck "Add a README" — the repository MUST be empty
6. Click **Create**

### Step 1B.2: Mirror GitHub to Azure DevOps

Open a terminal (or Git Bash) and run:

```bash
cd ~
mkdir repo-mirror-temp
cd repo-mirror-temp
```

Clone your GitHub repository as a mirror:

```bash
git clone --mirror https://github.com/YOUR_GITHUB_USERNAME/YOUR_REPO.git
```

Navigate into the cloned repository:

```bash
cd YOUR_REPO.git
```

Push to Azure DevOps:

```bash
git push --mirror https://dev.azure.com/YOUR_ORG/YOUR_PROJECT/_git/YOUR_REPO
```

You may be prompted to authenticate with your Microsoft account.

### Step 1B.3: Verify the Mirror

1. Go to Azure DevOps → Repos → Files
2. You should see all your code, branches, and commit history

### Step 1B.4: Clean Up

```bash
cd ~
rm -rf repo-mirror-temp
```

**Now skip to Part 2** (Create a GitHub Personal Access Token) and continue from there. When you reach Part 4, skip it — you've already done the initial mirror.

---

## Part 1: Create the GitHub Repository (Scenario A)

> **Note:** If you started with an existing GitHub repo (Scenario B), skip to Part 2.

### Step 1.1: Log in to GitHub

1. Open your web browser
2. Go to **https://github.com**
3. Click **Sign in** (top right corner)
4. Enter your username/email and password
5. Complete any two-factor authentication if enabled

### Step 1.2: Create a New Empty Repository

1. Once logged in, click the **+** icon in the top right corner (next to your profile picture)
2. Click **New repository**
3. Fill in the repository details:
   - **Repository name**: Use the same name as your Azure DevOps repo (recommended for clarity)
   - **Description**: Optional - add a description if you want
   - **Visibility**: Choose **Private** (recommended) or **Public**
   - **⚠️ IMPORTANT**: Do NOT check any of these boxes:
     - ❌ Add a README file
     - ❌ Add .gitignore
     - ❌ Choose a license
   - The repository MUST be completely empty for the mirror to work
4. Click the green **Create repository** button
5. You'll see a page with setup instructions - **leave this browser tab open**, you'll need the URL later

### Step 1.3: Note Your GitHub Repository URL

On the page shown after creating the repo, you'll see your repository URL. It looks like:

```
https://github.com/YOUR-USERNAME/YOUR-REPO-NAME.git
```

Write this down or keep the tab open.

---

## Part 2: Create a GitHub Personal Access Token (PAT)

A Personal Access Token is like a password that allows Azure DevOps to push code to your GitHub repository.

### Step 2.1: Navigate to Token Settings

1. Click your **profile picture** in the top right corner of GitHub
2. Click **Settings** (near the bottom of the dropdown menu)
3. Scroll down the left sidebar and click **Developer settings** (at the very bottom)
4. In the left sidebar, click **Personal access tokens**
5. Click **Tokens (classic)**
6. Click **Generate new token**
7. Click **Generate new token (classic)**

### Step 2.2: Configure the Token

1. **Note**: Enter a descriptive name, e.g., `Azure DevOps Mirror - [RepoName]`
2. **Expiration**: Choose an expiration period
   - For convenience, select **Custom** and set a date far in the future (e.g., 1 year)
   - ⚠️ Note: You'll need to regenerate and update the token when it expires
3. **Select scopes** - check the following boxes:
   - ✅ **repo** (this automatically selects all sub-items under it)
     - ✅ repo:status
     - ✅ repo_deployment
     - ✅ public_repo
     - ✅ repo:invite
     - ✅ security_events
   - ✅ **workflow** (needed because we'll create GitHub Actions)
4. Scroll down and click **Generate token**

### Step 2.3: Copy and Save the Token

1. **⚠️ CRITICAL**: You will only see this token ONCE. After you leave this page, you cannot see it again!
2. Click the **copy icon** (📋) next to the token to copy it
3. **Save it somewhere secure** - paste it into:
   - A password manager (recommended)
   - A secure note
   - A temporary text file (delete after setup is complete)

The token looks something like this:
```
ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

---

## Part 3: Create an Azure DevOps Personal Access Token (PAT)

This token allows GitHub to push code to your Azure DevOps repository.

### Step 3.1: Navigate to Azure DevOps

1. Open a new browser tab
2. Go to **https://dev.azure.com**
3. Sign in with your Microsoft account if not already signed in
4. Click on your organization name to enter it

### Step 3.2: Access Personal Access Tokens

1. Click the **User settings** icon (⚙️) in the top right corner (next to your profile picture)
2. Click **Personal access tokens**

### Step 3.3: Create a New Token

1. Click **+ New Token**
2. Fill in the token details:
   - **Name**: Enter a descriptive name, e.g., `GitHub Mirror - [RepoName]`
   - **Organization**: Select your organization (should be pre-selected)
   - **Expiration**: 
     - Click **Custom defined**
     - Set an expiration date (maximum is 1 year)
   - **Scopes**: Select **Custom defined**, then:
     - Scroll down to find **Code**
     - Check ✅ **Code** → **Read & write**
3. Click **Create**

### Step 3.4: Copy and Save the Token

1. **⚠️ CRITICAL**: Just like GitHub, you will only see this token ONCE!
2. Click the **copy icon** (📋) to copy the token
3. **Save it somewhere secure** (same place as your GitHub token)

The token looks something like this:
```
xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

---

## Part 4: Initial Repository Mirror (One-Time Setup)

> **Note:** If you started with an existing GitHub repo (Scenario B / Part 1B), skip this section — you've already done the initial mirror.

Now we'll copy all the code and history from Azure DevOps to GitHub.

### Step 4.1: Open a Terminal/Command Prompt

**On Windows:**
1. Press `Windows key + R`
2. Type `cmd` and press Enter

**Or use Git Bash (recommended):**
1. Right-click on your Desktop
2. Select **Git Bash Here** (if available)

### Step 4.2: Create a Temporary Working Folder

```bash
cd %USERPROFILE%
mkdir repo-mirror-temp
cd repo-mirror-temp
```

Or in Git Bash / PowerShell:
```bash
cd ~
mkdir repo-mirror-temp
cd repo-mirror-temp
```

### Step 4.3: Clone Azure DevOps Repository as a Mirror

Replace the placeholders with your actual values:
- `{org}` = Your Azure DevOps organization name
- `{project}` = Your Azure DevOps project name
- `{repo}` = Your Azure DevOps repository name

```bash
git clone --mirror https://dev.azure.com/{org}/{project}/_git/{repo}.git
```

**Example:**
```bash
git clone --mirror https://dev.azure.com/MyCompany/Sway/_git/Sway.git
```

You may be prompted to authenticate:
- A browser window might open - sign in with your Microsoft account
- Or enter your Azure DevOps credentials

### Step 4.4: Navigate into the Cloned Repository

```bash
cd {repo}.git
```

**Example:**
```bash
cd Sway.git
```

### Step 4.5: Push to GitHub

Replace placeholders:
- `{github-username}` = Your GitHub username
- `{github-repo}` = Your GitHub repository name
- `{github-pat}` = Your GitHub Personal Access Token from Part 2

```bash
git remote set-url origin https://{github-username}:{github-pat}@github.com/{github-username}/{github-repo}.git
git push --mirror
```

**Example:**
```bash
git remote set-url origin https://petersmith:ghp_abc123xyz@github.com/petersmith/Sway.git
git push --mirror
```

### Step 4.6: Verify the Mirror

1. Go to your GitHub repository in the browser
2. Refresh the page
3. You should now see all your code, branches, and commit history

### Step 4.7: Clean Up

Go back to your home directory and delete the temporary folder:

```bash
cd ..
cd ..
rmdir /s /q repo-mirror-temp
```

Or in Git Bash:
```bash
cd ~
rm -rf repo-mirror-temp
```

---

## Part 5: Set Up Azure DevOps → GitHub Sync (Azure Pipeline)

This creates an automatic process that pushes changes to GitHub whenever you push to Azure DevOps.

> **📝 Note for Git Flow users:** If you're using Git Flow, your default branch is likely `develop` rather than `main`. You'll need the pipeline file to exist in **both** branches for syncing to work on both. The easiest approach is to create the file in one branch, then cherry-pick the commit into the other branch. Instructions for this are included at the end of this section.

> **📝 How this pipeline works:** The pipeline only pushes the **single branch** that triggered it. This is safer than pushing all branches at once, which can cause accidental deletions. Each branch push triggers its own pipeline run.

### Step 5.1: Store GitHub Token in Azure DevOps

1. Go to **https://dev.azure.com/{org}/{project}** (your project)
2. Click **Pipelines** in the left sidebar
3. Click **Library** (under Pipelines)
4. Click **+ Variable group**
5. Configure the variable group:
   - **Variable group name**: `GitHub-Mirror-Secrets`
   - Click **+ Add** under Variables
   - **Name**: `GITHUB_PAT`
   - **Value**: Paste your GitHub Personal Access Token from Part 2
   - Click the **lock icon** 🔒 next to the value to make it secret
6. Click **Save** at the top

### Step 5.2: Create the Pipeline File

You need to add a pipeline configuration file to your repository.

1. Go to **Repos** → **Files** in Azure DevOps
2. Make sure you're on the `main` branch (or your default branch)
3. Click the **three dots** (...) next to the repository name
4. Click **+ New** → **File**
5. **Name**: `azure-pipelines-mirror.yml`
6. Paste the following content:

```yaml
# Azure DevOps to GitHub Mirror Pipeline
trigger:
  branches:
    include:
      - '*'

pool:
  vmImage: 'ubuntu-latest'

variables:
  - group: GitHub-Mirror-Secrets

steps:
  - checkout: self
    persistCredentials: true
    fetchDepth: 0

  - script: |
      COMMIT_MSG=$(git log -1 --pretty=%B)
      if echo "$COMMIT_MSG" | grep -q "\[skip mirror\]"; then
        echo "##[warning]Skipping mirror - commit originated from GitHub"
        exit 0
      fi
      
      BRANCH_NAME=$(echo "$(Build.SourceBranch)" | sed 's|refs/heads/||')
      echo "Mirroring branch: $BRANCH_NAME"
      
      git config user.email "azure-pipeline@mirror.local"
      git config user.name "Azure DevOps Mirror"
      
      git checkout -b $BRANCH_NAME
      git remote add github https://$(GITHUB_PAT)@github.com/GITHUB_USERNAME/GITHUB_REPO.git
      git push github $BRANCH_NAME --force
      
    displayName: 'Mirror to GitHub'
    env:
      GITHUB_PAT: $(GITHUB_PAT)
```

7. **⚠️ IMPORTANT**: Before saving, replace these placeholders in the script:
   - `GITHUB_USERNAME` → Your actual GitHub username
   - `GITHUB_REPO` → Your actual GitHub repository name

8. Click **Commit** (top right)
9. In the commit dialog:
   - Commit message: `Add GitHub mirror pipeline`
   - Select **Commit directly to the main branch**
   - Click **Commit**

### Step 5.3: Create the Pipeline

1. Go to **Pipelines** in the left sidebar
2. Click **Create Pipeline** (or **New pipeline** if you have existing pipelines)
3. Select **Azure Repos Git**
4. Select your repository
5. Select **Existing Azure Pipelines YAML file**
6. In the dropdown, select `/azure-pipelines-mirror.yml`
7. Click **Continue**
8. Review the YAML (don't run it yet)
9. Click the **down arrow** next to "Run" and select **Save**

### Step 5.4: Authorize the Variable Group

1. Go to **Pipelines** → **Library**
2. Click on your `GitHub-Mirror-Secrets` variable group
3. Click the **Pipeline permissions** tab
4. Click **+** and add your mirror pipeline
5. Or click **Open access** to allow all pipelines (simpler)

### Step 5.5: Run a Test

1. Go to **Pipelines**
2. Click on your mirror pipeline
3. Click **Run pipeline**
4. Click **Run**
5. Wait for it to complete (should take less than a minute)
6. Check your GitHub repository - it should match Azure DevOps

### Step 5.6: Git Flow Users — Add Pipeline to Other Branches

If you're using Git Flow and created the pipeline file in `main`, you need to also add it to `develop` (and vice versa). Here are three ways to do this:

> **⚠️ Important:** After cherry-picking or merging, always verify the pipeline file has the correct content. Open `azure-pipelines-mirror.yml` and confirm it contains `Mirroring branch:` in the script. If it says `Pushing all branches to GitHub...` instead, you have the old broken version — replace it manually with the correct YAML from step 5.2.

**Option A: Using SourceTree (recommended)**

1. Open your repository in SourceTree
2. Make sure you have the latest by clicking **Fetch**
3. Double-click on `develop` to make it the active branch (or whichever branch needs the file)
4. In the log/history view, find the commit "Add GitHub mirror pipeline" on the `main` branch
5. Right-click on that commit
6. Click **Cherry Pick**
7. Confirm the cherry-pick
8. Click **Push** to push `develop` to Azure DevOps

**Option B: Using Git command line**

```bash
# Get the latest from the server
git fetch origin

# Switch to develop
git checkout develop

# Find the commit hash from main (look for your pipeline commit)
git log main --oneline -5

# Cherry-pick it (replace abc1234 with the actual hash from the previous command)
git cherry-pick abc1234

# Push to Azure DevOps
git push origin develop
```

**Option C: Manually create the file**

1. Go to Azure DevOps → Repos → Files
2. Switch to the `develop` branch (dropdown at the top)
3. Click the **three dots** (...) next to the repo name → **New** → **File**
4. Name it `azure-pipelines-mirror.yml`
5. Copy-paste the exact same YAML content from the `main` branch version
6. Commit it

Now pushes to both `main` and `develop` will trigger the mirror.

---

## Part 6: Set Up GitHub → Azure DevOps Sync (GitHub Action)

This creates an automatic process that pushes changes to Azure DevOps whenever you push to GitHub.

> **📝 Note for Git Flow users:** Same as Part 5 — you'll need the workflow file in both `main` and `develop`. Create it in one branch first, then cherry-pick to the other.

### Step 6.1: Store Azure DevOps Token in GitHub

1. Go to your GitHub repository
2. Click **Settings** (tab at the top of the repo, not your profile settings)
3. In the left sidebar, click **Secrets and variables**
4. Click **Actions**
5. Click **New repository secret**
6. Configure the secret:
   - **Name**: `AZURE_PAT`
   - **Secret**: Paste your Azure DevOps Personal Access Token from Part 3
7. Click **Add secret**

### Step 6.2: Create the GitHub Actions Workflow File

1. In your GitHub repository, make sure you're on the **main** branch (check the branch dropdown on the left)
   - **Git Flow users**: You can start with either `main` or `develop` — you'll cherry-pick to the other branch in step 6.4
2. Click **Add file** → **Create new file**
2. In the filename field, type: `.github/workflows/mirror-to-azure.yml`
   - This will automatically create the folders
3. Paste the following content:

```yaml
# GitHub to Azure DevOps Mirror Workflow
# This workflow automatically mirrors commits to Azure DevOps

name: Mirror to Azure DevOps

on:
  push:
    branches:
      - '**'  # Trigger on all branches
  workflow_dispatch:  # Allow manual trigger

jobs:
  mirror:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Fetch all history for all branches

      - name: Mirror to Azure DevOps
        run: |
          echo "Checking if this commit should be mirrored..."
          
          # Skip if this commit came from Azure DevOps (prevents infinite loop)
          COMMIT_MSG=$(git log -1 --pretty=%B)
          if echo "$COMMIT_MSG" | grep -q "\[skip mirror\]"; then
            echo "⚠️ Skipping mirror - commit contains [skip mirror] tag"
            exit 0
          fi
          
          if echo "$COMMIT_MSG" | grep -q "\[azure-mirror\]"; then
            echo "⚠️ Skipping mirror - commit originated from Azure DevOps"
            exit 0
          fi
          
          echo "Configuring git..."
          git config user.email "github-action@mirror.local"
          git config user.name "GitHub Actions Mirror"
          
          echo "Adding Azure DevOps remote..."
          git remote add azure https://${{ secrets.AZURE_PAT }}@dev.azure.com/AZURE_ORG/AZURE_PROJECT/_git/AZURE_REPO
          
          echo "Pushing all branches to Azure DevOps..."
          git push azure --all --force
          
          echo "Pushing all tags to Azure DevOps..."
          git push azure --tags --force
          
          echo "✅ Mirror complete!"
```

4. **⚠️ IMPORTANT**: Before saving, replace these placeholders:
   - `AZURE_ORG` → Your Azure DevOps organization name
   - `AZURE_PROJECT` → Your Azure DevOps project name  
   - `AZURE_REPO` → Your Azure DevOps repository name

5. Click **Commit changes...**
6. In the dialog:
   - Commit message: `Add Azure DevOps mirror workflow`
   - Select **Commit directly to the main branch**
   - Click **Commit changes**

### Step 6.3: Verify the Workflow

1. Click **Actions** tab in your GitHub repository
2. You should see the "Mirror to Azure DevOps" workflow
3. It may have already run (triggered by the commit you just made)
4. Click on the workflow run to see the details
5. If successful, you'll see a green checkmark ✅

### Step 6.4: Git Flow Users — Add Workflow to Other Branches

If you're using Git Flow, cherry-pick the workflow file to your other main branch:

**Option A: Using SourceTree (recommended)**

1. Open the **GitHub** repository in SourceTree
   - ⚠️ **Important**: This must be the GitHub repo, not the Azure DevOps repo you may have open from earlier steps
   - If you haven't added it yet: **File** → **New** → **Clone from URL** → paste `https://github.com/YOUR-USERNAME/YOUR-REPO.git`
2. Click **Fetch** to get the latest
3. Double-click on the branch that needs the file (e.g., `develop` if you created it in `main`)
4. Find the commit "Add Azure DevOps mirror workflow" in the log
5. Right-click on that commit → **Cherry Pick**
6. Confirm the cherry-pick
7. Click **Push**

**Option B: Using Git command line**

```bash
# Navigate to a folder for the GitHub repo (or use your existing local clone)
git clone https://github.com/YOUR-USERNAME/YOUR-REPO.git
cd YOUR-REPO

git fetch origin
git checkout develop
git log main --oneline -5
git cherry-pick <commit-hash>
git push origin develop
```
(Swap `develop` and `main` if you created the file in `develop`)

**Option C: Manually create the file**

Create the file through the GitHub web interface in the other branch, copying the same content.

---

## Part 7: Testing the Complete Setup

### Test 1: Azure DevOps → GitHub

1. Go to Azure DevOps → Repos → Files
2. Click on any file (e.g., README.md or create a new test file)
3. Click **Edit**
4. Make a small change (add a comment or a line)
5. Click **Commit**
6. Enter a commit message like `Test: Azure DevOps to GitHub sync`
7. Click **Commit**
8. Go to **Pipelines** and watch the mirror pipeline run
9. Once complete, check GitHub - your change should appear

### Test 2: GitHub → Azure DevOps

1. Go to your GitHub repository
2. Click on any file
3. Click the **pencil icon** (✏️) to edit
4. Make a small change
5. Click **Commit changes...**
6. Enter a commit message like `Test: GitHub to Azure DevOps sync`
7. Click **Commit changes**
8. Go to **Actions** tab and watch the workflow run
9. Once complete, check Azure DevOps - your change should appear

### Final Step: Sync Your Local Repository

After all the setup and testing, your local repository may be behind. Pull all branches to get everything in sync:

**In SourceTree:**
1. Checkout `main` → Click **Pull**
2. Checkout `develop` → Click **Pull**
3. Checkout any feature branches → Click **Pull**

**In command line:**
```bash
git checkout main && git pull
git checkout develop && git pull
git checkout feature/YourFeatureBranch && git pull
```

Now your local machine is fully in sync with both remotes.

---

## Part 8: Troubleshooting

### Problem: Pipeline/Action Fails with Authentication Error

**Symptoms**: Error messages mentioning "401", "403", "authentication failed"

**Solutions**:
1. Verify your PAT hasn't expired
2. Ensure the PAT has the correct permissions:
   - GitHub PAT: needs `repo` and `workflow` scopes
   - Azure DevOps PAT: needs `Code (Read & write)`
3. Check that you copied the entire PAT (no extra spaces)
4. Regenerate the PAT and update it in the secrets/library

### Problem: Pipeline Runs in an Infinite Loop

**Symptoms**: Pipelines keep triggering each other endlessly

**Solutions**:
1. Ensure both YAML files have the `[skip mirror]` and marker tag checks
2. Add `[skip mirror]` to your commit message to manually break the loop
3. Temporarily disable one of the pipelines in settings

### Problem: "Repository Not Found" Error

**Symptoms**: Error says repository doesn't exist

**Solutions**:
1. Double-check the URLs in your YAML files
2. Ensure there are no typos in organization/project/repo names
3. Verify the PAT belongs to an account with access to the repo

### Problem: Force Push Rejected

**Symptoms**: Error about protected branches or force push denied

**Solutions**:
1. **GitHub**: Go to Settings → Branches → Branch protection rules → Edit the rule for your branch → Uncheck "Do not allow force pushes"
2. **Azure DevOps**: Go to Repos → Branches → Click the three dots on your branch → Branch policies → Turn off any policies that prevent force push

### Problem: Only Some Branches Sync

**Symptoms**: Main branch syncs but other branches don't

**Solutions**:
1. Ensure both YAML files have `branches: include: '*'` (or `'**'`)
2. Run the pipeline/action manually after pushing to a new branch

### Problem: Push Rejected - "fetch first" Error

**Symptoms**: When pushing from SourceTree, you get an error saying "Updates were rejected because the remote contains work that you do not have locally"

**Solutions**:
This happens when the sync has added commits to the remote that you don't have locally. Simply:
1. **Pull** first to get the latest changes
2. Then **Push** your changes

If you get merge conflicts, you may need to resolve them first, or use force push if appropriate.

### ⚠️ Warning: Never Use `git push --mirror` in Pipelines

If you're modifying the pipeline scripts, **never** use `git push --mirror` in a pipeline context. Azure DevOps pipelines check out code in a "detached HEAD" state with only remote-tracking branches — using `--mirror` will delete any branches on the target that aren't present locally, which can wipe out your entire repository structure.

Always push specific branches: `git push <remote> <branch> --force`

### Disaster Recovery: Restore GitHub from Azure DevOps

If something goes wrong and GitHub loses branches, but Azure DevOps is intact:

```bash
cd ~
mkdir restore-github
cd restore-github

# Clone everything from Azure DevOps
git clone --mirror https://dev.azure.com/YOUR_ORG/YOUR_PROJECT/_git/YOUR_REPO.git

cd YOUR_REPO.git

# Push everything to GitHub
git push --mirror https://YOUR_GITHUB_USERNAME:YOUR_GITHUB_PAT@github.com/YOUR_USERNAME/YOUR_REPO.git
```

This restores all branches and tags from Azure DevOps to GitHub.

### Disaster Recovery: Restore Azure DevOps from GitHub

If Azure DevOps loses branches but GitHub is intact, do the reverse:

```bash
cd ~
mkdir restore-azure
cd restore-azure

# Clone everything from GitHub
git clone --mirror https://github.com/YOUR_USERNAME/YOUR_REPO.git

cd YOUR_REPO.git

# Push everything to Azure DevOps
git push --mirror https://dev.azure.com/YOUR_ORG/YOUR_PROJECT/_git/YOUR_REPO.git
```

---

## Part 9: Day-to-Day Usage

### Normal Workflow

Once set up, just work normally:

1. **Working in Azure DevOps (Visual Studio)**
   - Make changes, commit, push as usual
   - Changes automatically appear in GitHub within 1-2 minutes

2. **Working in GitHub (Claude Code Web)**
   - Make changes, commit, push as usual
   - Changes automatically appear in Azure DevOps within 1-2 minutes

### Pulling Latest Changes Locally

Since code can come from either source, **always pull before starting work**:

**In SourceTree:**
1. Click **Fetch** to see what's available
2. Click **Pull** to get the latest changes

**In command line:**
```bash
git pull origin <your-branch>
```

**⚠️ Important:** If you forget to pull and try to push, you may get a "rejected - fetch first" error. Just pull first, then push.

### If You Need to Stop the Mirror Temporarily

Just add `[skip mirror]` anywhere in your commit message:

```bash
git commit -m "WIP changes [skip mirror]"
```

This commit won't trigger the mirror to the other repository.

---

## Part 10: Working with Claude Code Web

If you set up this mirror to work with Claude Code's web interface (which only supports GitHub, not Azure DevOps), here's how the workflow typically works.

### The Typical Claude Code Web Workflow

1. **You** work on a feature branch in Azure DevOps (e.g., `ImplementAuthenticationFrontend`)
2. The mirror syncs your feature branch to GitHub
3. **Claude Code Web** reads the repo from GitHub, does its work, and creates a new branch (typically under a `/claude/` folder, e.g., `claude/add-login-validation`)
4. The mirror syncs Claude's branch back to Azure DevOps
5. **You** review Claude's changes in your normal environment (Visual Studio, SourceTree, etc.)
6. If you accept the changes, you merge Claude's branch into your feature branch

### Important: Feature Branches Need the Pipeline File

For this workflow to function, your feature branch must contain the `azure-pipelines-mirror.yml` file. Otherwise:
- Your feature branch won't sync to GitHub → Claude Code can't see your latest work
- Claude's branches won't sync back to Azure DevOps → you can't review them easily

**Before starting work with Claude Code on a feature branch:**

Make sure to merge `develop` (which has the pipeline file) into your feature branch:

**Using SourceTree:**
1. Checkout your feature branch
2. Right-click on `develop`
3. Select **Merge develop into current branch**
4. Push

**Using command line:**
```bash
git checkout your-feature-branch
git merge develop
git push origin your-feature-branch
```

### Future Feature Branches

Any new feature branches you create from `develop` will automatically inherit the pipeline file — no extra steps needed. This is only a concern for feature branches that existed before you added the mirror setup.

### Branches Created by Claude Code

When Claude Code creates branches (e.g., `claude/fix-authentication`), those branches inherit the pipeline file from the branch they were based on. So as long as your feature branch has the file, Claude's branches will sync automatically.

---

## Part 11: Maintenance

### When PATs Expire

You'll need to regenerate tokens and update them:

1. **GitHub PAT**:
   - Generate a new token (Part 2)
   - Go to Azure DevOps → Pipelines → Library → GitHub-Mirror-Secrets
   - Update the `GITHUB_PAT` value

2. **Azure DevOps PAT**:
   - Generate a new token (Part 3)
   - Go to GitHub → Settings → Secrets and variables → Actions
   - Update the `AZURE_PAT` secret

### Checking Sync Status

- **Azure DevOps**: Go to Pipelines and check recent runs
- **GitHub**: Go to Actions tab and check recent workflow runs

### Disabling the Mirror

If you need to stop the synchronization:

1. **Azure DevOps**: Go to Pipelines → Your mirror pipeline → Three dots → Settings → Disable
2. **GitHub**: Go to Actions → Mirror to Azure DevOps → Three dots → Disable workflow

---

## Quick Reference Card

| Item | Location |
|------|----------|
| GitHub PAT | github.com → Settings → Developer settings → Personal access tokens |
| Azure DevOps PAT | dev.azure.com → User settings (gear icon) → Personal access tokens |
| GitHub Secret | GitHub repo → Settings → Secrets and variables → Actions |
| Azure DevOps Secret | Azure DevOps → Pipelines → Library → Variable groups |
| Azure Pipeline | `azure-pipelines-mirror.yml` in repo root |
| GitHub Action | `.github/workflows/mirror-to-azure.yml` |

---

## Summary

You now have:
- ✅ A GitHub repository mirroring your Azure DevOps repository
- ✅ Automatic sync from Azure DevOps → GitHub (Azure Pipeline)
- ✅ Automatic sync from GitHub → Azure DevOps (GitHub Action)
- ✅ Loop prevention to avoid infinite triggers

**Work in Visual Studio + Azure DevOps** when you prefer that environment.
**Work in GitHub + Claude Code Web** when you want the better Claude Code experience.

Both repositories stay in sync automatically! 🎉

---

*Last updated: December 2024. This guide has been tested with a real repository and includes lessons learned from actual setup issues.*
