package com.example.amfootball.ui.screens.chat

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
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.remote.dtos.chat.ChatRoom
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.previewsMocks.ChatListMocks
import com.example.amfootball.ui.viewModel.chat.ChatViewModel

/**
 * Ecrã principal de listagem de conversas (Inbox de Chats).
 *
 * Este ecrã atua como um container "Stateful" (com estado). Ele conecta-se ao [ChatViewModel]
 * para observar os dados e passa-os para o [ChatListContent] desenhar a UI.
 *
 * @param navController Controlador de navegação para transitar para o ecrã de detalhe.*
 * @param viewModel Hilt-injected ViewModel injetado via Hilt que fornece o fluxo de dados `rooms`.
 * */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatListScreen(
    navController: NavHostController,
    viewModel: ChatViewModel = hiltViewModel()
) {
    val rooms by viewModel.rooms.collectAsState()

    ChatListContent(
        rooms = rooms,
        navController = navController
    )
}

/**
 * Conteúdo visual da lista de chats (Stateless).
 *
 * Responsável apenas por desenhar a UI com base na lista de [rooms] fornecida.
 * Separa a lógica de visualização da lógica de negócio, facilitando testes e Previews.
 *
 * @param rooms Lista de salas de chat a exibir.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatListContent(
    rooms: List<ChatRoom>,
    navController: NavHostController
) {
    Scaffold { paddingValues ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .background(MaterialTheme.colorScheme.background)
        ) {
            if (rooms.isEmpty()) {
                item {
                    Box(
                        modifier = Modifier
                            .fillParentMaxSize()
                            .padding(24.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            text = "Sem chats",
                            modifier = Modifier.testTag("tag_empty_list_chat")
                        )
                    }
                }
            } else {
                items(rooms) { chat ->
                    ChatItem(chat, navController = navController)
                    HorizontalDivider(
                        modifier = Modifier
                            .padding(start = 80.dp),
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

@Preview(
    name = "Lista com Chats",
    locale = "pt-rPT",
    showBackground = true)
@Preview(
    name = "List With Chats",
    locale = "en",
    showBackground = true)
@Composable
fun PreviewChatListPopulated() {
    MaterialTheme {
        ChatListContent(
            rooms = ChatListMocks.mockChatRooms,
            navController = rememberNavController()
        )
    }
}

@Preview(
    name = "Lista Vazia",
    locale = "pt-rPT",
    showBackground = true)
@Preview(
    name = "Empty List",
    locale = "en",
    showBackground = true)
@Composable
fun PreviewChatListEmpty() {
    MaterialTheme {
        ChatListContent(
            rooms = emptyList(),
            navController = rememberNavController()
        )
    }
}

@Preview(
    name = "Item Individual",
    locale = "pt-rPT",
    showBackground = true)
@Preview(name = "Individual Item",
    locale = "en",
    showBackground = true)
@Composable
fun PreviewChatItem() {
    MaterialTheme {
        ChatItem(
            chat = ChatRoom(id = "99", name = "Grupo de Futebol"),
            navController = rememberNavController()
        )
    }
}