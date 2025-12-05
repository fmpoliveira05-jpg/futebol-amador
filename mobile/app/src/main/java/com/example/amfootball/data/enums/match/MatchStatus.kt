package com.example.amfootball.data.enums.match

import androidx.annotation.StringRes
import com.example.amfootball.R

/**
 * Enumerado que define os possíveis estados do ciclo de vida de um jogo (Match).
 *
 * Este enum centraliza a lógica de estados do jogo e associa cada estado diretamente
 * ao seu recurso de texto correspondente, facilitando a exibição na Interface do Utilizador (UI)
 * de forma localizada.
 *
 * @property stringId O identificador do recurso de string ([com.example.amfootball.R.string]) que contém a descrição legível deste estado.
 * A anotação [@StringRes] garante, em tempo de compilação, que o valor passado é uma referência válida de string.
 */
enum class MatchStatus(@StringRes val stringId: Int) {
    /**
     * O jogo está agendado para uma data futura e confirmado, mas ainda não iniciou.
     */
    SCHEDULED(R.string.match_status_scheduled),

    /**
     * O jogo está a decorrer neste exato momento (Live).
     */
    IN_PROGRESS(R.string.match_status_in_progress),

    /**
     * O jogo foi concluído e o resultado é final.
     */
    DONE(R.string.match_status_done),

    /**
     * O jogo foi adiado para uma data posterior (Remarcado).
     */
    POST_PONED(R.string.match_status_post_poned),

    /**
     * O jogo foi cancelado definitivamente e não se realizará.
     */
    CANCELED(R.string.match_status_canceled)
}