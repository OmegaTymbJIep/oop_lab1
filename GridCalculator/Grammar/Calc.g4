grammar Calc;

expression
   : additionExpression
   ;

additionExpression
   : multiplicationExpression ((op=('+'|'-')) multiplicationExpression)*
   ;

multiplicationExpression
   : unaryExpression ((op=('*'|'/'|'%')) unaryExpression)*
   ;

unaryExpression
   : op=('+'|'-')* term
   ;

term
   : '(' expression ')'
   | NUMBER
   | CellPointer
   ;

NUMBER: [0-9]+ ('.' [0-9]+)?;

CellPointer: '$' [A-Za-z]+ '$' [1-9] [0-9]* ;

WS: [ \t\r\n]+ -> skip ;