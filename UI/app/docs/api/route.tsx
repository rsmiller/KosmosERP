
import { NextRequest } from "next/server";
import { chromium } from "playwright";

export const runtime = "nodejs"; // ensure Node runtime (not Edge)

export async function GET(req: NextRequest) {
  const { searchParams } = new URL(req.url);
  let url = searchParams.get("url");
  if (!url) {
    return new Response("Missing ?url=", { status: 400 });
  }

  if (!url.startsWith('http://') && !url.startsWith('https://')) {
    const origin = req.nextUrl.origin;
    url = `${origin}${url.startsWith('/') ? url : `/${url}`}`;
  }
  //console.log(url)

  const browser = await chromium.launch({
    args: ["--no-sandbox", "--disable-setuid-sandbox"],
  });
  try {
    const page = await browser.newPage();

    // Recommended: render with print CSS and wait for fonts/styles
    await page.emulateMedia({ media: "print" });
    await page.goto(url, { waitUntil: "networkidle", timeout: 60_000 });
    // Wait for web fonts (important with Chakra/custom fonts)
    // If fonts aren't used, this resolves immediately.
    // biome-ignore lint/suspicious/noThenProperty: harmless
    await page.evaluate(() => (document as any).fonts?.ready);

    const pdf = await page.pdf({
      format: "A4",
      printBackground: true,
      margin: { top: "20mm", right: "15mm", bottom: "20mm", left: "15mm" },
    });

    return new Response(new Uint8Array(pdf), {
      status: 200,
      headers: {
        "Content-Type": "application/pdf",
        "Content-Disposition": 'inline; filename="document.pdf"',
        "Cache-Control": "no-store",
      },
    });
  } finally {
    await browser.close();
  }
}
