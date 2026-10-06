"use client"

import createCache from "@emotion/cache"
import { CacheProvider } from "@emotion/react"
import { useServerInsertedHTML } from "next/navigation"
import { useState } from "react"

/**
 * Emotion (used by Chakra) for the App Router.
 *
 * Without this, server rendering writes each component's <style data-emotion>
 * tag inline in <body>. Emotion's client cache moves those tags to <head> as
 * it starts, while React is hydrating the same markup, so React finds HTML
 * that doesn't match and throws error #418 (BUG-005).
 *
 * This collects the styles inserted during server rendering and hands them to
 * Next.js through useServerInsertedHTML, which writes them into <head>, outside
 * the React tree. `compat` stops Emotion from also writing them inline. The
 * same pattern as MUI's AppRouterCacheProvider.
 */
export function EmotionRegistry({ children }: { children: React.ReactNode }) {
  const [registry] = useState(() => {
    const cache = createCache({ key: "css" })
    cache.compat = true

    let inserted: { name: string; isGlobal: boolean }[] = []
    const prevInsert = cache.insert
    cache.insert = (...args) => {
      const [selector, serialized] = args
      if (cache.inserted[serialized.name] === undefined) {
        // Global styles (<Global>, Chakra's resets) are inserted with no selector.
        inserted.push({ name: serialized.name, isGlobal: !selector })
      }
      return prevInsert(...args)
    }

    const flush = () => {
      const flushed = inserted
      inserted = []
      return flushed
    }

    return { cache, flush }
  })

  useServerInsertedHTML(() => {
    const names = registry.flush()
    if (names.length === 0) return null

    let styles = ""
    let dataEmotion = registry.cache.key
    const globals: { name: string; style: string }[] = []

    for (const { name, isGlobal } of names) {
      const style = registry.cache.inserted[name]
      if (typeof style !== "string") continue
      if (isGlobal) {
        globals.push({ name, style })
      } else {
        styles += style
        dataEmotion += ` ${name}`
      }
    }

    return (
      <>
        {globals.map(({ name, style }) => (
          <style key={name} data-emotion={`${registry.cache.key}-global ${name}`} dangerouslySetInnerHTML={{ __html: style }} />
        ))}
        {styles && <style data-emotion={dataEmotion} dangerouslySetInnerHTML={{ __html: styles }} />}
      </>
    )
  })

  return <CacheProvider value={registry.cache}>{children}</CacheProvider>
}
