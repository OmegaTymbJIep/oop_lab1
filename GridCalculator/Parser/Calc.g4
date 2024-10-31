grammar Calc;

expression
   :
   |   binaryExpression
   |   unaryExpression
   ;

// Examples 10 * 20, 30 - 40, etc.
binaryExpression
   :
   |   left=unaryExpression op=('*'|'/') right=unaryExpression
   |   left=unaryExpression op=('+'|'-') right=unaryExpression
   ;

// Example -1.2, +20, -(10 + 20), etc.
unaryExpression
   :
   |   op=('+'|'-')* right=term
   ;

term
   :
   |   '(' expression ')'
   |   NUMBER
   |   CellPointer
   ;

// Example 10, 2.0, 30, 1.9, etc.
NUMBER: [0-9]+ ('.' [0-9]+)?;

// Example $AB$10 the same with $ab$10 - column AB, row 10.
CellPointer: '$' [A-Za-z]+ '$' [0-9]+ ;

// Skip whitespaces.
WS: [ \t\r\n]+ -> skip ;