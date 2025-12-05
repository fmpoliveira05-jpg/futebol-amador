package com.example.amfootball.ui.screens.Chat

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.chat.ChatRoom
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.chat.ChatViewModel

/**
 * Ecrã principal de listagem de conversas (Inbox de Chats).
 *
 * Este ecrã exibe todas as salas de chat ([ChatRoom]) em que o utilizador participa.
 * Utiliza o [ChatViewModel] para observar em tempo real a lista de salas via `StateFlow`.
 *
 * **Estrutura:**
 * - **Scaffold:** Fornece a estrutura base (suporta FloatingActionButton para criar novas conversas, atualmente comentado).
 * - **LazyColumn:** Renderiza a lista de chats de forma eficiente.
 * - **Empty State:** Exibe uma mensagem "Sem chats" caso a lista esteja vazia.
 *
 * @param navController Controlador de navegação para transitar para o ecrã de detalhe do chat (Single Chat).
 * @param viewModel ViewModel injetado via Hilt que fornece os dados das salas (`rooms`).
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatListScreen(
    navController: NavHostController,
    viewModel: ChatViewModel = hiltViewModel()
) {
    val rooms by viewModel.rooms.collectAsState()
    Scaffold{ paddingValues ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .background(MaterialTheme.colorScheme.background)
        ) {
            if (rooms.isEmpty()) {
                item { Text("Sem chats") }
            } else {
                items(rooms) { chat ->
                    ChatItem(chat, navController = navController)
                    HorizontalDivider(
                        modifier = Modifier.padding(start = 80.dp),
                        thickness = 0.5.dp,
                        color = Color.LightGray.copy(alpha = 0.4f)
                    )
                }
            }
        }
    }
}

/**
 * Componente que representa um item individual na lista de conversas.
 *
 * Exibe o avatar (atualmente uma inicial gerada) e o nome da sala.
 * Ao clicar, navega para a rota [Routes.PlayerRoutes.SINGLE_CHAT] passando o ID da sala.
 *
 * **Nota de Implementação Futura:**
 * Existem secções de código comentadas preparadas para exibir:
 * - Última mensagem e hora.
 * - Indicador de "Online".
 * - Contador de mensagens não lidas (Badge).
 * Estes campos dependem da evolução do objeto [ChatRoom] ou da integração com [ItemListChatDto].
 *
 * @param chat O objeto de dados da sala de chat.
 * @param modifier Modificador de layout.
 * @param navController Controlador para navegar para a conversa específica.
 */
@Composable
fun ChatItem(
    chat: ChatRoom,
    navController: NavHostController,
    modifier: Modifier = Modifier,
) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .clickable {
                navController.navigate(
                    Routes.PlayerRoutes.SINGLE_CHAT.route.replaceAfter(
                        "/",
                        chat.id
                    )
                )
            }
            .padding(horizontal = 16.dp, vertical = 12.dp)
            .testTag(stringResource(id = R.string.tag_item_list_chat)),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Box {
            Surface(
                modifier = Modifier.size(56.dp),
                shape = CircleShape,
                color = MaterialTheme.colorScheme.primaryContainer
            ) {
                Box(contentAlignment = Alignment.Center) {
                    val initial = chat.name.firstOrNull()?.toString()?.uppercase() ?: "?"
                    Text(
                        text = initial,
                        fontSize = 24.sp,
                        fontWeight = FontWeight.SemiBold,
                        color = MaterialTheme.colorScheme.onPrimaryContainer
                    )
                }
            }
        }

        Spacer(modifier = Modifier.width(16.dp))

        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = chat.name,
                style = MaterialTheme.typography.bodyLarge,
                fontWeight = FontWeight.SemiBold,
                color = MaterialTheme.colorScheme.onBackground
            )
        }
    }
}

/*
@Preview(
    name = "Chat List - English",
    locale = "en",
    showBackground = true,
    showSystemUi = true
)
@Preview(
    name = "Chat List - Portuguese",
    locale = "pt",
    showBackground = true,
    showSystemUi = true
)
@Composable
fun ChatListScreenPreview() {
    // Dados fictícios para o Preview
    val dummyRooms = listOf(
        ChatRoom(id = "1", name = "Equipa A vs Equipa B"),
        ChatRoom(id = "2", name = "Grupo de Treino"),
        ChatRoom(id = "3", name = "João Silva")
    )

    MaterialTheme {
        ChatListContent(
            rooms = dummyRooms,
            onChatClick = {}
        )
    }
}

@Preview(name = "Empty State", showBackground = true)
@Composable
fun ChatListEmptyPreview() {
    MaterialTheme {
        ChatListContent(
            rooms = emptyList(),
            onChatClick = {}
        )
    }
}
 */