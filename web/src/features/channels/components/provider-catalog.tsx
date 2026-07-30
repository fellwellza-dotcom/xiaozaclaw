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
import { Plus } from 'lucide-react'
import { useTranslation } from 'react-i18next'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

import { useChannels } from './channels-provider'

const PROVIDERS = [
  { name: 'OpenAI', type: 1 },
  { name: 'Anthropic', type: 14 },
  { name: 'Google Gemini', type: 24 },
  { name: 'DeepSeek', type: 43 },
  { name: 'xAI', type: 48 },
  { name: 'OpenRouter', type: 20 },
  { name: 'Azure OpenAI', type: 3 },
  { name: 'Mistral', type: 42 },
  { name: '智谱 AI', type: 26 },
  { name: '通义千问', type: 17 },
  { name: '百度千帆', type: 46 },
  { name: '腾讯混元', type: 23 },
  { name: '月之暗面', type: 25 },
  { name: 'MiniMax', type: 35 },
  { name: '硅基流动', type: 40 },
  { name: '火山方舟', type: 45 },
  { name: 'Ollama', type: 4 },
] as const

export function ProviderCatalog() {
  const { t } = useTranslation()
  const { beginChannelCreate } = useChannels()

  return (
    <Card className='mb-4'>
      <CardHeader>
        <CardTitle>{t('Popular model providers')}</CardTitle>
        <CardDescription>
          {t('Choose a provider to prefill a secure connection form. You only need its API key.')}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className='grid gap-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4'>
          {PROVIDERS.map((provider) => (
            <Button
              key={provider.type}
              variant='outline'
              className='justify-between'
              onClick={() => beginChannelCreate(provider.type)}
            >
              <span className='truncate'>{provider.name}</span>
              <Plus data-icon='inline-end' />
            </Button>
          ))}
        </div>
      </CardContent>
    </Card>
  )
}
