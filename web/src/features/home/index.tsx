/*
Copyright (C) 2023-2026 QuantumNous

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.

For commercial licensing, please contact support@quantumnous.com
*/
import { Link, Navigate } from '@tanstack/react-router'
import { useCallback, useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'

import { PublicLayout } from '@/components/layout'
import { Footer } from '@/components/layout/components/footer'
import { RichContent } from '@/components/rich-content'
import { Button } from '@/components/ui/button'
import { useTheme } from '@/context/theme-provider'
import { isLikelyHtml } from '@/lib/content-format'
import { useAuthStore } from '@/stores/auth-store'

import { useHomePageContent } from './hooks'

export function Home() {
  const { i18n, t } = useTranslation()
  const iframeRef = useRef<HTMLIFrameElement>(null)
  const { resolvedTheme } = useTheme()
  const { auth } = useAuthStore()
  const isAuthenticated = !!auth.user
  const { content, isLoaded, isUrl } = useHomePageContent()

  const syncIframePreferences = useCallback(() => {
    try {
      iframeRef.current?.contentWindow?.postMessage(
        { themeMode: resolvedTheme },
        '*'
      )
      iframeRef.current?.contentWindow?.postMessage(
        { lang: i18n.language },
        '*'
      )
    } catch {
      // Cross-origin frames may reject access while navigating.
    }
  }, [i18n.language, resolvedTheme])

  useEffect(() => {
    if (isUrl) {
      syncIframePreferences()
    }
  }, [isUrl, syncIframePreferences])

  if (!isLoaded) {
    return (
      <PublicLayout showMainContainer={false}>
        <main className='flex min-h-screen items-center justify-center'>
          <div className='text-muted-foreground'>{t('Loading...')}</div>
        </main>
      </PublicLayout>
    )
  }

  if (content) {
    if (isUrl) {
      return (
        <PublicLayout showMainContainer={false}>
          {/*
            allow-top-navigation-by-user-activation: the custom home page URL is
            admin-configured (trusted); this lets its target="_top" nav/menu links
            navigate the top-level window on user click. The default sandbox blocks
            this on desktop, while some mobile browsers allow it via allow-popups,
            causing inconsistent behavior. This token only permits user-activated
            top-level navigation and does NOT grant same-origin access.
          */}
          <iframe
            ref={iframeRef}
            src={content}
            className='h-screen w-full border-none'
            title={t('Custom Home Page')}
            sandbox='allow-forms allow-popups allow-popups-to-escape-sandbox allow-scripts allow-top-navigation-by-user-activation'
            onLoad={syncIframePreferences}
          />
        </PublicLayout>
      )
    }

    const contentIsHtml = isLikelyHtml(content)

    if (contentIsHtml) {
      return (
        <PublicLayout showMainContainer={false}>
          <RichContent
            mode='html'
            htmlVariant='isolated'
            content={content}
            className='custom-home-content'
          />
        </PublicLayout>
      )
    }

    return (
      <PublicLayout>
        <div className='mx-auto max-w-6xl px-4 py-8'>
          <RichContent
            mode='markdown'
            content={content}
            className='custom-home-content'
          />
        </div>
      </PublicLayout>
    )
  }

  if (isAuthenticated) {
    return <Navigate to='/workspace' />
  }

  return (
    <PublicLayout showMainContainer={false}>
      <main className='from-primary/10 via-background to-background flex min-h-[calc(100svh-var(--app-header-height,0px))] items-center bg-linear-to-br px-4 py-12 sm:px-6'>
        <section className='mx-auto grid w-full max-w-5xl gap-8 lg:grid-cols-[1.1fr_0.9fr] lg:items-center'>
          <div className='space-y-6'>
            <div className='bg-primary/10 text-primary inline-flex rounded-full px-3 py-1 text-sm font-medium'>
              {t('AI workspace')}
            </div>
            <div className='space-y-3'>
              <h1 className='text-4xl font-semibold tracking-tight sm:text-5xl'>
                {t('AI workspace')}
              </h1>
              <p className='text-muted-foreground max-w-xl text-lg leading-8'>
                {t('Choose a model and start working. Your gateway stays in the background.')}
              </p>
            </div>
            <div className='flex flex-wrap gap-3'>
              <Button size='lg' render={<Link to='/sign-in' />}>
                {t('Sign In')}
              </Button>
              <Button size='lg' variant='outline' render={<Link to='/setup' />}>
                {t('Get Started')}
              </Button>
            </div>
          </div>
          <div className='bg-card/85 grid gap-3 rounded-3xl border p-5 shadow-sm backdrop-blur sm:grid-cols-3 lg:grid-cols-1'>
            <LandingStep number='1' title={t('Connect model providers')} />
            <LandingStep number='2' title={t('Choose a model')} />
            <LandingStep number='3' title={t('AI workspace')} />
          </div>
        </section>
      </main>
      <Footer />
    </PublicLayout>
  )
}

function LandingStep(props: { number: string; title: string }) {
  return (
    <div className='flex items-center gap-3 rounded-2xl border bg-background p-4'>
      <span className='bg-primary text-primary-foreground flex size-7 shrink-0 items-center justify-center rounded-full text-sm font-semibold'>
        {props.number}
      </span>
      <span className='font-medium'>{props.title}</span>
    </div>
  )
}
