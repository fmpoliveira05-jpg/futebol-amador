package com.example.amfootball.utils.extensions

import java.util.Locale

/**
 * Função de extensão para [Double] que formata o número para exibir exatamente uma casa decimal.
 *
 * Utiliza [String.format] com o [Locale] atual do dispositivo para garantir que o separador decimal
 * é o ponto (padrão EN) ou a vírgula (padrão PT/EU), consoante a configuração do utilizador.
 *
 * Exemplo:
 * - Se o Locale for PT, 12.34 se torna "12,3" e 12.37 se torna "12,4".
 * - Se o Locale for EN, 12.34 se torna "12.3" e 12.37 se torna "12.4".
 *
 * @return Uma [String] formatada com uma única casa decimal, arredondada se necessário.
 */
fun Double.toOneDecimal(): String {
    return String.format(Locale.getDefault(), "%.1f", this)
}