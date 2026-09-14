This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## Getting Started

### Environment Setup

1. Copy the environment example file:
   ```bash
   cp .env.example .env.local
   ```

2. Update the `NEXT_PUBLIC_API_BASE_URL` in `.env.local` to point to your API backend (default is `http://localhost:5213`)

### Docker Deployment

When deploying with Docker, you can inject the environment variable in several ways:

#### Option 1: Docker Compose
```yaml
services:
  ui:
    build: .
    environment:
      - NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com
```

#### Option 2: Docker Run
```bash
docker run -e NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com your-image
```

#### Option 3: Environment File
```bash
# Create .env.production
echo "NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com" > .env.production

# Use with Docker
docker run --env-file .env.production your-image
```

**Important**: The `NEXT_PUBLIC_` prefix is required for Next.js to expose the variable to the browser/client-side code.

### Running the Development Server

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.
