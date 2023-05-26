# WebCrawler

O objetivo desse WebCrawler é obter os campos:
"IP Adress", "Port", "Country" e "Protocol" do site "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc".

Salvei o resultado da extração em um arquivo JSON, que é armazenado assim que termina a execução do programa. Além de todas as páginas extraídas em HTML.

Salvei os seguintes dados em um banco de dados:
1. Data de início da execução.
2. Data de término da execução.
3. Quantidade de páginas processadas durante a extração.
4. Quantidade total de linhas extraídas em todas as páginas.
5. Arquivo JSON gerado. (O arquivo JSON salvei como BSON, somente para mostrar o conceito de um objeto dentro do MongoDB, caso quisesse deixar como um JSON puro, poderia deixar como string).

Observações:
1. O valor "Port" só era disponibilizado no HTML caso a página estivesse 100% carregada, então tive que instanciar uma execução do Chrome, para poder pegar esse valor.
2. Durante a execução, enfrentei dificuldades ao atender um dos requisitos, que era implementar o WebCrawler como multithread. Essa dificuldade foi devido as instâncias do Chrome, 
que não funcionaram conforme o esperado. Como resultado, decidi remover a parte do código relacionada ao multithreading.
