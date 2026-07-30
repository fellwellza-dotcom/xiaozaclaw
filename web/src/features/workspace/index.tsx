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
import { Link } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { Bot, KeyRound, Send, Settings2, Sparkles } from 'lucide-react'
import { type FormEvent, useState, useTransition } from 'react'
import { useTranslation } from 'react-i18next'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { fetchActiveChatKey } from '@/features/chat/hooks/use-active-chat-key'
import { getUserModels } from '@/lib/api'
import { ROLE } from '@/lib/roles'
import { useAuthStore } from '@/stores/auth-store'

type WorkspaceMessage = {
  id: string
  role: 'user' | 'assistant'
  content: string
}

type ChatCompletionResponse = {
  choices?: Array<{ message?: { content?: string | null } }>
  error?: { message?: string }
}

async function requestChatCompletion(
  apiKey: string,
  model: string,
  messages: WorkspaceMessage[]
) {
  const response = await fetch('/v1/chat/completions', {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${apiKey}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      model,
      stream: false,
      messages: messages.map(({ role, content }) => ({ role, content })),
    }),
  })

  const payload = (await response.json()) as ChatCompletionResponse
  if (!response.ok) {
    throw new Error(payload.error?.message || 'Chat request failed')
  }

  const content = payload.choices?.[0]?.message?.content
  if (!content) {
    throw new Error('The model returned an empty response')
  }
  return content
}

export function Workspace() {
  const { t } = useTranslation()
  const userRole = useAuthStore((state) => state.auth.user?.role ?? ROLE.USER)
  const [selectedModel, setSelectedModel] = useState('')
  const [draft, setDraft] = useState('')
  const [messages, setMessages] = useState<WorkspaceMessage[]>([])
  const [error, setError] = useState<string | null>(null)
  const [isPending, startTransition] = useTransition()

  const modelsQuery = useQuery({
    queryKey: ['workspace-models'],
    queryFn: async () => {
      const result = await getUserModels()
      if (!result.success) {
        throw new Error(result.message || 'Unable to load available models')
      }
      return result.data ?? []
    },
    staleTime: 60_000,
  })

  const apiKeyQuery = useQuery({
    queryKey: ['workspace-active-api-key'],
    queryFn: fetchActiveChatKey,
    staleTime: 5 * 60_000,
  })

  const availableModels = modelsQuery.data ?? []
  const activeModel = selectedModel || availableModels[0] || ''
  const isAdministrator = userRole >= ROLE.ADMIN
  const isReady = Boolean(activeModel && apiKeyQuery.data)

  const sendMessage = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const content = draft.trim()
    if (!content || !activeModel || !apiKeyQuery.data || isPending) return

    const userMessage: WorkspaceMessage = {
      id: crypto.randomUUID(),
      role: 'user',
      content,
    }
    const conversation = [...messages, userMessage]
    setDraft('')
    setError(null)
    setMessages(conversation)

    startTransition(async () => {
      try {
        const reply = await requestChatCompletion(
          apiKeyQuery.data,
          activeModel,
          conversation
        )
        setMessages((current) => [
          ...current,
          { id: crypto.randomUUID(), role: 'assistant', content: reply },
        ])
      } catch (requestError) {
        setError(
          requestError instanceof Error
            ? requestError.message
            : t('Unable to send the message. Please try again.')
        )
      }
    })
  }

  return (
    <main className='flex h-full min-h-0 flex-col bg-muted/20'>
      <header className='flex flex-wrap items-center justify-between gap-3 border-b bg-background px-4 py-3 sm:px-6'>
        <div className='flex items-center gap-3'>
          <div className='bg-primary/10 text-primary flex size-9 items-center justify-center rounded-xl'>
            <Sparkles className='size-5' />
          </div>
          <div>
            <h1 className='font-semibold'>{t('AI workspace')}</h1>
            <p className='text-muted-foreground text-xs'>
              {t('Choose a model and start working. Your gateway stays in the background.')}
            </p>
          </div>
        </div>
        <Select value={activeModel || null} onValueChange={setSelectedModel}>
          <SelectTrigger className='w-52 max-w-full'>
            <SelectValue placeholder={t('Choose a model')} />
          </SelectTrigger>
          <SelectContent>
            {availableModels.map((model) => (
              <SelectItem key={model} value={model}>
                {model}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </header>

      {!isReady ? (
        <WorkspaceSetup
          isAdministrator={isAdministrator}
          isLoading={modelsQuery.isPending || apiKeyQuery.isPending}
          problem={modelsQuery.error || apiKeyQuery.error}
        />
      ) : (
        <section className='mx-auto flex min-h-0 w-full max-w-4xl flex-1 flex-col px-4 py-5 sm:px-6'>
          <div className='min-h-0 flex-1 space-y-4 overflow-y-auto pb-5'>
            {messages.length === 0 ? (
              <EmptyConversation onSelect={setDraft} />
            ) : (
              messages.map((message) => (
                <article
                  key={message.id}
                  className={
                    message.role === 'user'
                      ? 'ml-auto max-w-[85%] rounded-2xl rounded-br-md bg-primary px-4 py-3 text-primary-foreground'
                      : 'max-w-[85%] rounded-2xl rounded-bl-md border bg-background px-4 py-3 shadow-sm'
                  }
                >
                  <p className='whitespace-pre-wrap break-words text-sm leading-6'>{message.content}</p>
                </article>
              ))
            )}
            {isPending && (
              <div className='text-muted-foreground flex items-center gap-2 text-sm'>
                <Bot className='size-4 animate-pulse' />
                {t('Thinking...')}
              </div>
            )}
          </div>

          <form onSubmit={sendMessage} className='space-y-2'>
            {error && <p className='text-destructive text-sm'>{error}</p>}
            <Textarea
              value={draft}
              onChange={(event) => setDraft(event.target.value)}
              placeholder={t('Ask anything. Enter to send, Shift+Enter for a new line.')}
              onKeyDown={(event) => {
                if (event.key === 'Enter' && !event.shiftKey) {
                  event.preventDefault()
                  event.currentTarget.form?.requestSubmit()
                }
              }}
              disabled={isPending}
              className='min-h-28 resize-none bg-background'
            />
            <div className='flex justify-end'>
              <Button type='submit' disabled={!draft.trim() || isPending}>
                <Send />
                {t('Send')}
              </Button>
            </div>
          </form>
        </section>
      )}
    </main>
  )
}

function WorkspaceSetup(props: {
  isAdministrator: boolean
  isLoading: boolean
  problem: unknown
}) {
  const { t } = useTranslation()
  const problemMessage =
    props.problem instanceof Error ? props.problem.message : undefined

  return (
    <section className='mx-auto flex w-full max-w-3xl flex-1 items-center px-4 py-8 sm:px-6'>
      <Card className='w-full'>
        <CardHeader>
          <CardTitle>{props.isLoading ? t('Preparing your workspace') : t('One quick setup before you start')}</CardTitle>
          <CardDescription>
            {props.isLoading
              ? t('Checking your available models and access key...')
              : t('Connect a model provider once. Your team can then select models here without handling API details.')}
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-4'>
          {problemMessage && <p className='text-destructive text-sm'>{problemMessage}</p>}
          <div className='grid gap-3 sm:grid-cols-2'>
            {props.isAdministrator && (
              <Button render={<Link to='/channels' />}>
                <Settings2 />
                {t('Connect model providers')}
              </Button>
            )}
            <Button variant='outline' render={<Link to='/keys' />}>
              <KeyRound />
              {t('Create or enable an access key')}
            </Button>
          </div>
          {!props.isAdministrator && (
            <p className='text-muted-foreground text-sm'>
              {t('Ask your administrator to connect a model provider, then create an access key from your account.')}
            </p>
          )}
        </CardContent>
      </Card>
    </section>
  )
}

function EmptyConversation(props: { onSelect: (value: string) => void }) {
  const { t } = useTranslation()
  const suggestions = [
    t('Summarize this document'),
    t('Write a customer reply'),
    t('Turn my notes into an action list'),
  ]

  return (
    <div className='flex min-h-full flex-col items-center justify-center py-12 text-center'>
      <div className='bg-primary/10 text-primary mb-4 flex size-12 items-center justify-center rounded-2xl'>
        <Bot className='size-6' />
      </div>
      <h2 className='text-xl font-semibold'>{t('What would you like to work on?')}</h2>
      <p className='text-muted-foreground mt-2 max-w-lg text-sm'>
        {t('Use the model selector above when you need a different capability.')}
      </p>
      <div className='mt-5 flex flex-wrap justify-center gap-2'>
        {suggestions.map((suggestion) => (
          <Button key={suggestion} variant='outline' size='sm' onClick={() => props.onSelect(suggestion)}>
            {suggestion}
          </Button>
        ))}
      </div>
    </div>
  )
}
