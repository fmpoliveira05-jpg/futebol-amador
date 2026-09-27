import { AbstractControl, ValidationErrors } from '@angular/forms';

/** Comprimentos aceites pela API (`PasswordConst`); o Firebase não aceita mais de 128. */
export const PALAVRA_PASSE_MIN = 10;
export const PALAVRA_PASSE_MAX = 128;

/** Texto mostrado nos formulários (igual à mensagem da API). */
export const REGRA_PALAVRA_PASSE =
  'Entre 10 e 128 caracteres, com maiúsculas, minúsculas, algarismos e símbolos.';

/** A mesma política que a API aplica (`PalavraPasseSeguraAttribute`). */
export function cumprePoliticaPalavraPasse(valor: string | null | undefined): boolean {
  if (!valor || valor.length < PALAVRA_PASSE_MIN || valor.length > PALAVRA_PASSE_MAX) {
    return false;
  }
  return /\p{Ll}/u.test(valor) && /\p{Lu}/u.test(valor) && /\p{Nd}/u.test(valor) && /[^\p{L}\p{Nd}\s]/u.test(valor);
}

/** Validador dos formulários reativos. Um campo vazio fica a cargo do `Validators.required`. */
export function palavraPasseSegura(controlo: AbstractControl<string>): ValidationErrors | null {
  return !controlo.value || cumprePoliticaPalavraPasse(controlo.value) ? null : { palavraPasseFraca: true };
}
