grammar Calc;

expression
   : additionExpression
   ;

additionExpression
   : multiplicationExpression ((op=('+'|'-')) multiplicationExpression)*
   ;

multiplicationExpression
   : powerExpression ((op=('*'|'/'|'%')) powerExpression)*
   ;

powerExpression
   : unaryExpression ( '**' unaryExpression)*
   ;

unaryExpression
   : (op=('+'|'-'))* term
   ;

term
   : '(' expression ')'
   | NUMBER
   | CellPointer
   | functionCall
   ;

functionCall
   : (INC | DEC) '(' expression ')'
   ;

NUMBER: [0-9]+ ('.' [0-9]+)?;

CellPointer: '$' [A-Za-z]+ '$' [1-9] [0-9]* ;

INC: 'inc';

DEC: 'dec';

WS: [ \t\r\n]+ -> skip ;