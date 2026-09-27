import { Component, ElementRef, OnDestroy, afterNextRender, inject, output, viewChild } from '@angular/core';
import { environment } from '../../environments/environment';

/** API mínima do script do Turnstile (`window.turnstile`). */
interface ApiTurnstile {
  render(elemento: HTMLElement, opcoes: Record<string, unknown>): string;
  reset(id?: string): void;
  remove(id: string): void;
}

declare global {
  interface Window {
    turnstile?: ApiTurnstile;
  }
}

const URL_SCRIPT = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit';
let carregamento: Promise<void> | null = null;

/** Carrega o script do Cloudflare uma única vez, só quando uma página com o widget abre. */
function carregarScript(): Promise<void> {
  if (!carregamento) {
    carregamento = new Promise<void>((resolve, reject) => {
      const script = document.createElement('script');
      script.src = URL_SCRIPT;
      script.async = true;
      script.onload = () => resolve();
      script.onerror = () => {
        carregamento = null;
        reject(new Error('Não foi possível carregar a verificação anti-robô.'));
      };
      document.head.appendChild(script);
    });
  }
  return carregamento;
}

/**
 * Widget do Cloudflare Turnstile (proteção contra robôs) nos formulários de login, registo e
 * recuperação da palavra-passe.
 *
 * Só aparece quando `environment.turnstileSiteKey` está definido; sem chave não carrega nada e
 * `token` nunca é emitido (a API também não o exige). O token vai no cabeçalho `X-Turnstile-Token`.
 */
@Component({
  selector: 'app-turnstile',
  template: `@if (ativo) { <div #caixa class="turnstile"></div> }`,
})
export class TurnstileComponent implements OnDestroy {
  /** Emite o token quando o desafio é resolvido e `null` quando expira ou falha. */
  readonly token = output<string | null>();

  protected readonly ativo = !!environment.turnstileSiteKey;
  private readonly caixa = viewChild<ElementRef<HTMLElement>>('caixa');
  private readonly host = inject(ElementRef<HTMLElement>);
  private widgetId: string | null = null;

  constructor() {
    afterNextRender(() => {
      const elemento = this.caixa()?.nativeElement;
      if (!this.ativo || !elemento) {
        return;
      }
      carregarScript()
        .then(() => {
          if (!window.turnstile || !this.host.nativeElement.isConnected) {
            return;
          }
          this.widgetId = window.turnstile.render(elemento, {
            sitekey: environment.turnstileSiteKey,
            language: 'pt-PT',
            callback: (t: string) => this.token.emit(t),
            'expired-callback': () => this.token.emit(null),
            'error-callback': () => this.token.emit(null),
          });
        })
        .catch(() => this.token.emit(null));
    });
  }

  /** Pede um desafio novo (os tokens só servem uma vez). */
  reiniciar(): void {
    if (this.widgetId && window.turnstile) {
      window.turnstile.reset(this.widgetId);
      this.token.emit(null);
    }
  }

  ngOnDestroy(): void {
    if (this.widgetId && window.turnstile) {
      window.turnstile.remove(this.widgetId);
    }
  }
}

/** Se a página exige o Turnstile (há site key). */
export const TURNSTILE_ATIVO = !!environment.turnstileSiteKey;
