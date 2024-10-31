grammar Calc;

expression
   : binaryExpression
   | unaryExpression
   ;

// Examples: 10 * 20, 30 - 40, etc.
binaryExpression
   : left=unaryExpression op=('*'|'/') right=unaryExpression
   | left=unaryExpression op=('+'|'-') right=unaryExpression
   ;

// Examples: -1.2, +20, -(10 + 20), etc.
unaryExpression
   : op=('+'|'-')* term
   ;

term
   : '(' expression ')'
   | NUMBER
   | CellPointer
   ;

// Examples: 10, 2.0, 30, 1.9, etc.
NUMBER: [0-9]+ ('.' [0-9]+)?;

// Example: $AB$10 (or $ab$10) - column AB, row 10.
CellPointer: '$' [A-Za-z]+ '$' [0-9]+ ;

// Skip whitespaces.
WS: [ \t\r\n]+ -> skip ;